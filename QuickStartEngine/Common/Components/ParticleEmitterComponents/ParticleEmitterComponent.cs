//
// ParticleSystem.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

using QuickStart.Entities;
using QuickStart.Interfaces;
using QuickStart.Graphics;

using QuickStart.ParticleSystem;

namespace QuickStart.Components
{
    /// <summary>
    /// The main component in charge of displaying particles.
    /// </summary>
    public class ParticleEmitterComponent : BaseComponent
    {
        public override ComponentType GetComponentType() { return ComponentType.ParticleEmitterComponent; }

        public static BaseComponent LoadFromDefinition(ContentManager content, string definitionPath, BaseEntity parent)
        {
            ParticleEmitterComponentDefinition compDef = content.Load<ParticleEmitterComponentDefinition>(definitionPath);

            ParticleEmitterComponent newComponent = new ParticleEmitterComponent(parent, compDef);
            return newComponent;
        }

        protected EffectParameter effectVelocityParameter;
        protected EffectParameter effectLightBrightnessParameter;

        /// <summary>
        /// We store all of the particle emitter settings from the component definition
        /// </summary>
        private ParticleEmitterComponentDefinition settings;

        // An array of particles, treated as a circular queue.
        private ParticleVertex[] particles;

        // A vertex buffer holding our particles. This contains the same data as
        // the particles array, but copied across to where the GPU can access it.
        private DynamicVertexBuffer vertexBuffer;

        // Index buffer turns sets of four vertices into particle quads (pairs of triangles).
        private IndexBuffer indexBuffer;

        // Vertex declaration describes the format of our ParticleVertex structure.
        static private VertexDeclaration vertexDeclaration;

        /// <summary>
        /// This render component's material
        /// </summary>
        private Material material;

        // The particles array and vertex buffer are treated as a circular queue.
        // Initially, the entire contents of the array are free, because no particles
        // are in use. When a new particle is created, this is allocated from the
        // beginning of the array. If more than one particle is created, these will
        // always be stored in a consecutive block of array elements. Because all
        // particles last for the same amount of time, old particles will always be
        // removed in order from the start of this active particle region, so the
        // active and free regions will never be intermingled. Because the queue is
        // circular, there can be times when the active particle region wraps from the
        // end of the array back to the start. The queue uses modulo arithmetic to
        // handle these cases. For instance with a four entry queue we could have:
        //
        //      0
        //      1 - first active particle
        //      2 
        //      3 - first free particle
        //
        // In this case, particles 1 and 2 are active, while 3 and 4 are free.
        // Using modulo arithmetic we could also have:
        //
        //      0
        //      1 - first free particle
        //      2 
        //      3 - first active particle
        //
        // Here, 3 and 0 are active, while 1 and 2 are free.
        //
        // But wait! The full story is even more complex.
        //
        // When we create a new particle, we add them to our managed particles array.
        // We also need to copy this new data into the GPU vertex buffer, but we don't
        // want to do that straight away, because setting new data into a vertex buffer
        // can be an expensive operation. If we are going to be adding several particles
        // in a single frame, it is faster to initially just store them in our managed
        // array, and then later upload them all to the GPU in one single call. So our
        // queue also needs a region for storing new particles that have been added to
        // the managed array but not yet uploaded to the vertex buffer.
        //
        // Another issue occurs when old particles are retired. The CPU and GPU run
        // asynchronously, so the GPU will often still be busy drawing the previous
        // frame while the CPU is working on the next frame. This can cause a
        // synchronization problem if an old particle is retired, and then immediately
        // overwritten by a new one, because the CPU might try to change the contents
        // of the vertex buffer while the GPU is still busy drawing the old data from
        // it. Normally the graphics driver will take care of this by waiting until
        // the GPU has finished drawing inside the VertexBuffer.SetData call, but we
        // don't want to waste time waiting around every time we try to add a new
        // particle! To avoid this delay, we can specify the SetDataOptions.NoOverwrite
        // flag when we write to the vertex buffer. This basically means "I promise I
        // will never try to overwrite any data that the GPU might still be using, so
        // you can just go ahead and update the buffer straight away". To keep this
        // promise, we must avoid reusing vertices immediately after they are drawn.
        //
        // So in total, our queue contains four different regions:
        //
        // Vertices between firstActiveParticle and firstNewParticle are actively
        // being drawn, and exist in both the managed particles array and the GPU
        // vertex buffer.
        //
        // Vertices between firstNewParticle and firstFreeParticle are newly created,
        // and exist only in the managed particles array. These need to be uploaded
        // to the GPU at the start of the next draw call.
        //
        // Vertices between firstFreeParticle and firstRetiredParticle are free and
        // waiting to be allocated.
        //
        // Vertices between firstRetiredParticle and firstActiveParticle are no longer
        // being drawn, but were drawn recently enough that the GPU could still be
        // using them. These need to be kept around for a few more frames before they
        // can be reallocated.

        private int firstActiveParticle;
        private int firstNewParticle;
        private int firstFreeParticle;
        private int firstRetiredParticle;

        // Store the current time, in seconds.
        private float currentTime;

        // Count how many times Draw has been called. This is used to know
        // when it is safe to retire old particles back into the free list.
        private int drawCounter;

        protected Color colorShiftMin = Color.LightGray;
        protected Color colorShiftMax = Color.White;

        private Vector3 emitterOffset = Vector3.Zero;
        private Vector3 particleOffsetRandomness = new Vector3(0.0f, 0.0f, 0.0f);

        private float timeSinceLastParticle = 0.0f;
        private float timePerParticle = 1 / 50.0f;
        private int particlesPerSecond = 50;

        // Shared random number generator.
        static Random random = new Random();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ParticleEmitterComponent(BaseEntity parent, ParticleEmitterComponentDefinition compDef)
            : base(parent)
        {
            ActivateComponent();

            this.settings = compDef;

            if (settings.MaterialPath.Length > 0)
            {
                LoadMaterial(settings.MaterialPath);
            }
            else
            {
                throw new Exception("A ParticleEmitterComponentDefinition must contain either a valid material file path");
            }

            InitializeParticleSettings(this.settings);
        }

        /// <summary>
        /// Initializes the component.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Loads a entity's material
        /// </summary>
        /// <param name="materialPath">File path to the material file</param>
        protected void LoadMaterial(string materialPath)
        {
            this.material = this.parentEntity.Game.Content.Load<Material>(materialPath);
        }
        
        /// <summary>
        /// Initializes a particles settings
        /// </summary>
        /// <param name="settings">settings to initialize particle to</param>        
        public void InitializeParticleSettings(ParticleEmitterComponentDefinition settings)
        {
            FinalizeParticleSettings();

            if (vertexDeclaration == null)
            {
                vertexDeclaration = ParticleVertex.VertexDeclaration;
            }

            this.particles = new ParticleVertex[this.settings.MaxParticles * 4];

            for (int i = 0; i < this.settings.MaxParticles; i++)
            {
                this.particles[i * 4 + 0].Corner = new Short2(-1, -1);
                this.particles[i * 4 + 1].Corner = new Short2(1, -1);
                this.particles[i * 4 + 2].Corner = new Short2(1, 1);
                this.particles[i * 4 + 3].Corner = new Short2(-1, 1);
            }

            this.vertexBuffer = new DynamicVertexBuffer(this.parentEntity.Game.GraphicsDevice, ParticleVertex.VertexDeclaration,
                                                        settings.MaxParticles * 4, BufferUsage.WriteOnly);

            // Initialize the vertex buffer contents. This is necessary in order to correctly restore any existing particles after a lost device.
            this.vertexBuffer.SetData(this.particles);

            // Create and populate the index buffer.
            ushort[] indices = new ushort[this.settings.MaxParticles * 6];

            for (int i = 0; i < this.settings.MaxParticles; i++)
            {
                indices[i * 6 + 0] = (ushort)(i * 4 + 0);
                indices[i * 6 + 1] = (ushort)(i * 4 + 1);
                indices[i * 6 + 2] = (ushort)(i * 4 + 2);

                indices[i * 6 + 3] = (ushort)(i * 4 + 0);
                indices[i * 6 + 4] = (ushort)(i * 4 + 2);
                indices[i * 6 + 5] = (ushort)(i * 4 + 3);
            }

            this.indexBuffer = new IndexBuffer(this.parentEntity.Game.GraphicsDevice, typeof(short),
                                               indices.Length, BufferUsage.WriteOnly);

            this.indexBuffer.SetData(indices);
        }

        /// <summary>
        /// Helper for loading and initializing the particle effect.
        /// </summary>
        void FinalizeParticleSettings()
        {
            this.settings.RotateSpeed = new Vector2(this.settings.MinRotateSpeed, this.settings.MaxRotateSpeed);
            this.settings.StartSize = new Vector2(this.settings.MinStartSize, this.settings.MaxStartSize);
            this.settings.EndSize = new Vector2(this.settings.MinEndSize, this.settings.MaxEndSize);
            this.settings.MinColorVect = this.settings.MinColor.ToVector4();
            this.settings.MaxColorVect = this.settings.MaxColor.ToVector4();

            this.material.CurrentTechnique = "Particles";
        }

        /// <summary>
        /// Initialize the emitter settings
        /// </summary>
        /// <param name="emitterOffset">Particles will emit with this offset</param>
        /// <param name="particlesPerSecond">Amount of particles created per second</param>
        /// <param name="particleOffsetRandomness">Random X, Y, and Z offsets for the starting position of each particle</param>
        public void InitializeEmitterSettings(Vector3 emitterOffset, int particlesPerSecond, Vector3 particleOffsetRandomness )
        {
            this.emitterOffset = emitterOffset;
            this.particleOffsetRandomness = particleOffsetRandomness;

            this.timePerParticle = 1.0f / particlesPerSecond;
            this.particlesPerSecond = particlesPerSecond;
        }

        /// <summary>
        /// Update the component
        /// </summary>
        /// <param name="gameTime">XNA Timing snapshot</param>
        public override void Update(GameTime gameTime)
        {
            if (gameTime == null)
                throw new ArgumentNullException("gameTime");

            if (this.vertexBuffer == null)
                return;

            this.currentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            RunEmitter(gameTime);

            RetireActiveParticles();
            FreeRetiredParticles();

            // If we let our timer go on increasing for ever, it would eventually
            // run out of floating point precision, at which point the particles
            // would render incorrectly. An easy way to prevent this is to notice
            // that the time value doesn't matter when no particles are being drawn,
            // so we can reset it back to zero any time the active queue is empty.

            if (this.firstActiveParticle == this.firstFreeParticle)
                this.currentTime = 0;

            if (this.firstRetiredParticle == this.firstActiveParticle)
                this.drawCounter = 0;
        }

        void RunEmitter(GameTime gameTime)
        {
            this.timeSinceLastParticle += this.parentEntity.Game.PartialSecondsThisFrame;

            while (this.timeSinceLastParticle > this.timePerParticle)
            {
                this.timeSinceLastParticle -= this.timePerParticle;

                // @TODO: Rather than using Color.White, determine the actual lighting color
                if (!AddParticle(GenerateRandomPointFromEmitter(), Vector3.Zero, Color.White))
                {
                    break;
                }
            }
        }

        public Vector3 GenerateRandomPointFromEmitter()
        {
            Vector3 outVector = Vector3.Zero;
            outVector.X = (float)(random.NextDouble() * (this.particleOffsetRandomness.X * 2)) - this.particleOffsetRandomness.X;
            outVector.Y = (float)(random.NextDouble() * (this.particleOffsetRandomness.Y * 2)) - this.particleOffsetRandomness.Y;
            outVector.Z = (float)(random.NextDouble() * (this.particleOffsetRandomness.Z * 2)) - this.particleOffsetRandomness.Z;

            Vector3.Add(ref outVector, ref this.emitterOffset, out outVector);
            
            // Move emitter into world space
            Vector3 parentPos = this.parentEntity.Position;
            Vector3.Add(ref outVector, ref parentPos, out outVector);
            
            return outVector;
        }

        /// <summary>
        /// Helper for checking when active particles have reached the end of
        /// their life. It moves old particles from the active area of the queue
        /// to the retired section.
        /// </summary>
        void RetireActiveParticles()
        {
            float particleDuration = (float)this.settings.Duration.TotalSeconds;

            while (this.firstActiveParticle != this.firstNewParticle)
            {
                // Is this particle old enough to retire?
                // We multiply the active particle index by four, because each
                // particle consists of a quad that is made up of four vertices.
                float particleAge = this.currentTime - this.particles[this.firstActiveParticle * 4].Time;

                if (particleAge < particleDuration)
                    break;

                // Remember the time at which we retired this particle.
                this.particles[this.firstActiveParticle * 4].Time = this.drawCounter;

                // Move the particle from the active to the retired queue.
                ++this.firstActiveParticle;

                if (this.firstActiveParticle >= this.settings.MaxParticles)
                    this.firstActiveParticle = 0;
            }
        }


        /// <summary>
        /// Helper for checking when retired particles have been kept around long
        /// enough that we can be sure the GPU is no longer using them. It moves
        /// old particles from the retired area of the queue to the free section.
        /// </summary>
        void FreeRetiredParticles()
        {
            while (this.firstRetiredParticle != this.firstActiveParticle)
            {
                // Has this particle been unused long enough that
                // the GPU is sure to be finished with it?
                int age = drawCounter - (int)particles[firstRetiredParticle * 4].Time;

                // The GPU is never supposed to get more than 2 frames behind the CPU.
                // We add 1 to that, just to be safe in case of buggy drivers that
                // might bend the rules and let the GPU get further behind.
                if (age < 3)
                    break;

                // Move the particle from the retired to the free queue.
                ++this.firstRetiredParticle;

                if (this.firstRetiredParticle >= this.settings.MaxParticles)
                    this.firstRetiredParticle = 0;
            }
        }

        public override void QueryForChunks(ref RenderPassDesc desc)
        {
            // Restore the vertex buffer contents if the graphics device was lost.
            if (this.vertexBuffer.IsContentLost)
            {
                this.vertexBuffer.SetData(this.particles);
            }

            // If there are any particles waiting in the newly added queue, we'd better upload them to the GPU ready for drawing.
            if (this.firstNewParticle != this.firstFreeParticle)
            {
                AddNewParticlesToVertexBuffer();
            }

            // If there are any active particles, draw them now!
            if (this.firstActiveParticle != this.firstFreeParticle)
            {
                ParticleChunk chunk = this.parentEntity.Game.Graphics.AllocateParticleChunk();
                chunk.vertices = this.vertexBuffer;
                chunk.indices = this.indexBuffer;
                chunk.Material = this.material;
                chunk.Type = PrimitiveType.TriangleList;
                chunk.ParticleSettings = this.settings;
                chunk.CurrentTime = this.currentTime;

                chunk.StartVertexIndex = this.firstActiveParticle * 4;
                chunk.StartIndex = this.firstActiveParticle * 6;

                if (this.firstActiveParticle < this.firstFreeParticle)
                {
                    // If the active particles are all in one consecutive range, we can draw them all in a single call.                   
                    chunk.NumVerts = (this.firstFreeParticle - this.firstActiveParticle) * 4;
                    chunk.PrimitiveCount = (this.firstFreeParticle - this.firstActiveParticle) * 2;                    
                }
                else
                {
                    // If the active particle range wraps past the end of the queue
                    // back to the start, we must split them over two draw calls.                    
                    chunk.NumVerts = (this.settings.MaxParticles - this.firstActiveParticle) * 4;
                    chunk.PrimitiveCount = (this.settings.MaxParticles - this.firstActiveParticle) * 2;                    

                    if (this.firstFreeParticle > 0)
                    {
                        ParticleChunk chunkTwo = this.parentEntity.Game.Graphics.AllocateParticleChunk();
                        chunkTwo.vertices = this.vertexBuffer;
                        chunkTwo.indices = this.indexBuffer;
                        chunkTwo.StartVertexIndex = 0;
                        chunkTwo.NumVerts = this.firstFreeParticle * 4;
                        chunkTwo.StartIndex = 0;
                        chunkTwo.PrimitiveCount = this.firstFreeParticle * 2;                        
                        chunkTwo.Material = this.material;
                        chunkTwo.Type = PrimitiveType.TriangleList;
                        chunkTwo.ParticleSettings = this.settings;
                        chunkTwo.CurrentTime = this.currentTime;
                    }
                }
            }

            ++drawCounter;
        }

        /// <summary>
        /// Helper for uploading new particles from our managed
        /// array to the GPU vertex buffer.
        /// </summary>
        void AddNewParticlesToVertexBuffer()
        {
            int stride = ParticleVertex.SizeInBytes;

            if (this.firstNewParticle < this.firstFreeParticle)
            {
                // If the new particles are all in one consecutive range,
                // we can upload them all in a single call.
                this.vertexBuffer.SetData(this.firstNewParticle * stride * 4, this.particles,
                                        this.firstNewParticle * 4,
                                        (this.firstFreeParticle - this.firstNewParticle) * 4,
                                        stride, SetDataOptions.NoOverwrite);
            }
            else
            {
                // If the new particle range wraps past the end of the queue
                // back to the start, we must split them over two upload calls.
                this.vertexBuffer.SetData(this.firstNewParticle * stride * 4, this.particles,
                                        this.firstNewParticle * 4,
                                        (this.settings.MaxParticles - this.firstNewParticle) * 4,
                                        stride, SetDataOptions.NoOverwrite);

                if (this.firstFreeParticle > 0)
                {
                    this.vertexBuffer.SetData(0, this.particles,
                                            0, this.firstFreeParticle * 4,
                                            stride, SetDataOptions.NoOverwrite);
                }
            }

            // Move the particles we just uploaded from the new to the active queue.
            this.firstNewParticle = this.firstFreeParticle;
        }

        /// <summary>
        /// Adds a new particle to the system.
        /// </summary>
        public bool AddParticle(Vector3 position, Vector3 velocity, Color particleColor)
        {
            // Figure out where in the circular queue to allocate the new particle.
            int nextFreeParticle = this.firstFreeParticle + 1;

            if (nextFreeParticle >= this.settings.MaxParticles)
                nextFreeParticle = 0;

            // If there are no free particles, we just have to give up.
            if (nextFreeParticle == this.firstRetiredParticle)
                return false;

            // Adjust the input velocity based on how much
            // this particle system wants to be affected by it.
            velocity *= this.settings.EmitterVelocitySensitivity;

            // Add in some random amount of horizontal velocity.
            float horizontalVelocity = MathHelper.Lerp(this.settings.MinHorizontalVelocity,
                                                       this.settings.MaxHorizontalVelocity,
                                                       (float)random.NextDouble());

            double horizontalAngle = random.NextDouble() * MathHelper.TwoPi;

            velocity.X += horizontalVelocity * (float)Math.Cos(horizontalAngle);
            velocity.Z += horizontalVelocity * (float)Math.Sin(horizontalAngle);

            // Add in some random amount of vertical velocity.
            velocity.Y += MathHelper.Lerp(this.settings.MinVerticalVelocity,
                                          this.settings.MaxVerticalVelocity,
                                          (float)random.NextDouble());

            // Choose four random control values. These will be used by the vertex
            // shader to give each particle a different size, rotation, and color.
            Color randomValues = new Color((byte)random.Next(255),
                                       (byte)random.Next(255),
                                       (byte)random.Next(255),
                                       (byte)random.Next(255));

            // Fill in the particle vertex structure.
            for (int i = 0; i < 4; i++)
            {
                this.particles[firstFreeParticle * 4 + i].Position = position;
                this.particles[firstFreeParticle * 4 + i].Velocity = velocity;
                this.particles[firstFreeParticle * 4 + i].Random = randomValues;
                this.particles[firstFreeParticle * 4 + i].Time = currentTime;
            }

            this.firstFreeParticle = nextFreeParticle;

            return true;
        }

        /// <summary>
        /// Adds a new particle to the system.
        /// </summary>
        public void AddParticle(Vector3 position, Vector3 velocity)
        {
            // Figure out where in the circular queue to allocate the new particle.
            int nextFreeParticle = this.firstFreeParticle + 1;

            if (nextFreeParticle >= this.settings.MaxParticles)
                nextFreeParticle = 0;

            // If there are no free particles, we just have to give up.
            if (nextFreeParticle == this.firstRetiredParticle)
                return;

            // Adjust the input velocity based on how much
            // this particle system wants to be affected by it.
            velocity *= this.settings.EmitterVelocitySensitivity;

            // Add in some random amount of horizontal velocity.
            float horizontalVelocity = MathHelper.Lerp(this.settings.MinHorizontalVelocity,
                                                       this.settings.MaxHorizontalVelocity,
                                                       (float)random.NextDouble());

            double horizontalAngle = random.NextDouble() * MathHelper.TwoPi;

            velocity.X += horizontalVelocity * (float)Math.Cos(horizontalAngle);
            velocity.Y += horizontalVelocity * (float)Math.Sin(horizontalAngle);

            // Add in some random amount of vertical velocity.
            velocity.Z += MathHelper.Lerp(this.settings.MinVerticalVelocity,
                                          this.settings.MaxVerticalVelocity,
                                          (float)random.NextDouble());

            // Choose four random control values. These will be used by the vertex
            // shader to give each particle a different size, rotation, and color.
            Color randomValues = new Color((byte)random.Next(255),
                                           (byte)random.Next(255),
                                           (byte)random.Next(255),
                                           (byte)random.Next(255));

            // Fill in the particle vertex structure.
            for (int i = 0; i < 4; i++)
            {
                this.particles[firstFreeParticle * 4 + i].Position = position;
                this.particles[firstFreeParticle * 4 + i].Velocity = velocity;
                this.particles[firstFreeParticle * 4 + i].Random = randomValues;
                this.particles[firstFreeParticle * 4 + i].Time = currentTime;
            }

            this.firstFreeParticle = nextFreeParticle;
        }

        /// <summary>
        /// Handles a message sent to this component.
        /// </summary>
        /// <param name="message">Message to be handled</param>
        /// <returns>True, if handled, otherwise false</returns>
        public override bool ExecuteMessage(IMessage message)
        {
            if (message.UniqueTarget != this.parentEntity.UniqueID)
                return false;

            return false;
        }
    }
}
