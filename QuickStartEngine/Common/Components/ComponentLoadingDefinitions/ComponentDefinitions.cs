//
// ComponentDefinition.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using QuickStart.GeometricPrimitives;
using QuickStart.Graphics;
using QuickStart.Physics;

namespace QuickStart.Components
{
    public class RenderComponentDefinition
    {
        [ContentSerializer]
        public string ModelPath;

        [ContentSerializer]
        public string MaterialPath;

        [ContentSerializer]
        public GeometricPrimitiveType PrimitiveType;

        [ContentSerializer]
        public PreferredRenderOrder RenderOrder;

        [ContentSerializer]
        public bool CanCreateShadows;

        [ContentSerializer]
        public bool CanReceiveShadows;

        [ContentSerializer]
        public Vector4 MeshColor;

        [ContentSerializer]
        public bool RendersAsSky = false;

        [ContentSerializer]
        public float Opacity = 1.0f;


        // Dimensions are only used if a GeometricPrimitiveType is specified.

        [ContentSerializer]
        public float Height = 1.0f;

        [ContentSerializer]
        public float Width = 1.0f;

        [ContentSerializer]
        public float Depth = 1.0f;

        [ContentSerializer]
        public float Diameter = 1.0f;
    }

    /// <summary>
    /// This is used for both regular Physics and Phantom physics
    /// </summary>
    public class PhysicsComponentDefinition
    {
        [ContentSerializer]
        public ShapeType ShapeType;

        [ContentSerializer]
        public bool IsDynamic;

        [ContentSerializer]
        public bool AffectedByGravity = true;

        [ContentSerializer]
        public float Mass = 1.0f;

        [ContentSerializer]
        public CollisionGroups CollisionGroupType = CollisionGroups.CollideWithAll;

        [ContentSerializer]
        public string PhysicsModelPath;     // This is optional

        // Dimensions are only used if no physics model path is specified.

        [ContentSerializer]
        public float Height;

        [ContentSerializer]
        public float Width;

        [ContentSerializer]
        public float Depth;

        [ContentSerializer]
        public float Diameter;
    }

    public class CharacterPhysicsComponentDefinition
    {
        [ContentSerializer]
        public bool AffectedByGravity = true;

        [ContentSerializer]
        public float Mass;

        [ContentSerializer]
        public string PhysicsModelPath;     // This is optional, can be used only with with 'box' or 'sphere' shape.

        // Dimensions are only used if no physics model path is specified.

        [ContentSerializer]
        public float Height;

        [ContentSerializer]
        public float Width;

        [ContentSerializer]
        public float Depth;

        [ContentSerializer]
        public float Diameter;
    }

    public class CameraComponentDefinition
    {
        [ContentSerializer]
        public float AspectRatio = 1.3333f;

        [ContentSerializer]
        public int StartingZoomLevel = 1;

        [ContentSerializer]
        public float DefaultFOV = QSConstants.DefaultFOV;

        [ContentSerializer]
        public float StartingFOV = QSConstants.DefaultFOV;

        [ContentSerializer]
        public float NearPlane = QSConstants.DefaultNearPlane;

        [ContentSerializer]
        public float FarPlane = QSConstants.DefaultFarPlane;

        [ContentSerializer]
        public bool ForceFrustumAboveTerrain;
    }

    public class WaterComponentDefinition
    {
        [ContentSerializer]
        public string MaterialPath;

        [ContentSerializer]
        public int Width;

        [ContentSerializer]
        public int Length;

        [ContentSerializer]
        public Vector4 WaterColorLight;

        [ContentSerializer]
        public Vector4 WaterColorDark;

        [ContentSerializer]
        public float Reflectivity;
    }

    public class LightComponentDefinition
    {        
        [ContentSerializer]
        public Vector4 AmbientColor = new Vector4(0.3f, 0.3f, 0.3f, 1.0f);
        
        [ContentSerializer]
        public Vector4 SpecularColor = new Vector4(0.4f, 0.4f, 0.4f, 1.0f);

        [ContentSerializer]
        public Vector4 DiffuseColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

        /// <summary>
        /// Specular power of light
        /// </summary>
        [ContentSerializer]
        public float SpecularPower = 4.0f;

        [ContentSerializer]
        public Vector3 LightDirection = new Vector3(-.2f, -.4f, .2f);
    }

    public class ConstantMovementComponentDefinition
    {
        /// <summary>
        /// Direction of movement (units per second).        
        /// </summary>
        [ContentSerializer]
        public Vector3 MovementVector = Vector3.Zero;

        /// <summary>
        /// Frequency of movement (crests per second). Movement is in a sine wave pattern.
        /// Frequency is 1 / period. Where the period is defined as the length between crests in the wave.
        /// Leave this at 0.0f if you want a linear motion rather than a wave motion.
        /// </summary>
        [ContentSerializer]
        public float Frequency = 0.0f;

        /// <summary>
        /// Amplitude of movement. Movement is in a sine wave pattern.
        /// Amplitude is the height of the wave.
        /// Leave this at 0.0f if you want a linear motion rather than a wave motion.
        /// </summary>
        [ContentSerializer]
        public float Amplitude = 0.0f;
    }

    public class ConstantRotationComponentDefinition
    {
        /// <summary>
        /// This is the amount of rotation around the X axis that will be added to this object every second.
        /// </summary>
        [ContentSerializer]
        public float AmountXAxisPerSecond;

        /// <summary>
        /// This is the amount of rotation around the Y axis that will be added to this object every second.
        /// </summary>
        [ContentSerializer]
        public float AmountYAxisPerSecond;

        /// <summary>
        /// This is the amount of rotation around the Z axis that will be added to this object every second. 
        /// </summary>
        [ContentSerializer]
        public float AmountZAxisPerSecond;
    }

    public class ArcBallCameraInputComponentDefinition
    {
        /// <summary>
        /// Holds true/false whether the camera controls are inverted or not.
        /// </summary>
        [ContentSerializer]
        public bool Inverted = false;

        /// <summary>
        /// Amount of radians the camera is rotated around the UP axis from the world forward (+Z).
        /// </summary>
        [ContentSerializer]
        public float HorizontalAngle;

        /// <summary>
        /// Amount of radians the camera is rotated around the world right axis (X).
        /// </summary>
        [ContentSerializer]
        public float VerticalAngle;

        /// <summary>
        /// Lowest value that <see cref="verticalAngle"/> is allowed to go.
        /// </summary>
        [ContentSerializer]
        public float VerticalAngleMin = 0.01f;

        /// <summary>
        /// Highest value that <see cref="verticalAngle"/> is allowed to go.
        /// </summary>
        [ContentSerializer]
        public float VerticalAngleMax = MathHelper.Pi - 0.01f;

        /// <summary>
        /// Lowest value that <see cref="zoom"/> is allowed to go.
        /// </summary>
        [ContentSerializer]
        public float ZoomMin;

        /// <summary>
        /// Highest value that <see cref="zoom"/> is allowed to go.
        /// </summary>
        [ContentSerializer]
        public float ZoomMax;

        [ContentSerializer]
        public float StartingZoom;

        /// <summary>
        /// Modifier for gamepad trigger sensitivity for this specific camera.
        /// </summary>
        [ContentSerializer]
        public float TriggerModifier = 0.05f;

        /// <summary>
        /// Distance that the camera will look ahead of its target, along the camera's forward vector
        /// (flattened) to the X-Z plane.
        /// </summary>
        [ContentSerializer]
        public float LookAheadDistance = 0.0f; 
       
        /// <summary>
        /// Distance that the camera will look above its target, along the world UP axis.
        /// </summary>
        [ContentSerializer]
        public float LookAboveDistance = 0.0f;

        /// <summary>
        /// Modifier for left thumb stick sensitivity for this input component.
        /// </summary>
        [ContentSerializer]
        public float LeftThumbStickModifier = 3.0f;

        /// <summary>
        /// Modifier for right thumb stick sensitivity for this input component.
        /// </summary>
        [ContentSerializer]
        public float RightThumbStickModifier = 0.05f;
    }

    public class CharacterInputComponentDefinition
    {
        [ContentSerializer]
        public float MovementSpeed = 50.0f;

        [ContentSerializer]
        public float RotationSpeed = MathHelper.PiOver2;

        /// <summary>
        /// Modifier for left thumb stick sensitivity for this input component.
        /// </summary>
        [ContentSerializer]
        public float LeftThumbStickModifier = 3.0f;

        /// <summary>
        /// Modifier for right thumb stick sensitivity for this input component.
        /// </summary>
        [ContentSerializer]
        public float RightThumbStickModifier = 0.05f;
    }

    public class FreeCameraInputComponentDefinition
    {
        /// <summary>
        /// Holds true/false whether the camera controls are inverted or not.
        /// </summary>
        [ContentSerializer]
        public bool Inverted = false;

        /// <summary>
        /// Speed modifier to determine how much movement is sent to the parent entity.
        /// </summary>
        [ContentSerializer]        
        public float Speed = 100.0f;

        /// <summary>
        /// Speed is multiplied by this amount when the player holds down the left mouse button.
        /// </summary>
        [ContentSerializer]        
        public int TurboSpeedModifier = 4;

        /// <summary>
        /// Modifier for left thumb stick sensitivity for this input component.
        /// </summary>
        [ContentSerializer]
        public float LeftThumbStickModifier = 3.0f;

        /// <summary>
        /// Modifier for right thumb stick sensitivity for this input component.
        /// </summary>
        [ContentSerializer]
        public float RightThumbStickModifier = 0.05f;
    }

    public class ParticleEmitterComponentDefinition
    {
        [ContentSerializer]
        public string MaterialPath;        

        /// <summary>
        /// Maximum number of particles that can be displayed at one time.
        /// </summary>
        [ContentSerializer]
        public int MaxParticles = 100;

        /// <summary>
        /// How long these particles will last. 
        /// </summary>
        /// [ContentSerializer]
        public TimeSpan Duration = TimeSpan.FromSeconds(1);

        /// <summary>
        /// If greater than zero, some particles will last a shorter time than others. 
        /// </summary>
        [ContentSerializer]
        public float DurationRandomness = 0;

        /// <summary>
        /// Controls how much particles are influenced by the velocity of the object
        /// which created them. You can see this in action with the explosion effect,
        /// where the flames continue to move in the same direction as the source
        /// projectile. The projectile trail particles, on the other hand, set this
        /// value very low so they are less affected by the velocity of the projectile.
        /// </summary>
        [ContentSerializer]
        public float EmitterVelocitySensitivity = 1;

        /// <summary>
        /// Range of values controlling how much X and Z axis velocity to give each
        /// particle. Values for individual particles are randomly chosen from somewhere
        /// between these limits.
        /// </summary>
        [ContentSerializer]
        public float MinHorizontalVelocity = 0;

        /// <summary>
        /// Range of values controlling how much X and Z axis velocity to give each
        /// particle. Values for individual particles are randomly chosen from somewhere
        /// between these limits.
        /// </summary>
        [ContentSerializer]
        public float MaxHorizontalVelocity = 0;

        /// <summary>
        /// Range of values controlling how much Y axis velocity to give each particle.
        /// Values for individual particles are randomly chosen from somewhere between
        /// these limits.
        /// </summary>
        [ContentSerializer]
        public float MinVerticalVelocity = 0;

        /// <summary>
        /// Range of values controlling how much Y axis velocity to give each particle.
        /// Values for individual particles are randomly chosen from somewhere between
        /// these limits.
        /// </summary>
        [ContentSerializer]
        public float MaxVerticalVelocity = 0;

        /// <summary>
        /// Direction and strength of the gravity effect. Note that this can point in any
        /// direction, not just down! The fire effect points it upward to make the flames
        /// rise, and the smoke plume points it sideways to simulate wind.
        /// </summary>
        [ContentSerializer]
        public Vector3 Gravity = Vector3.Zero;

        /// <summary>
        /// Controls how the particle velocity will change over their lifetime. If set
        /// to 1, particles will keep going at the same speed as when they were created.
        /// If set to 0, particles will come to a complete stop right before they die.
        /// Values greater than 1 make the particles speed up over time.
        /// </summary>
        [ContentSerializer]
        public float EndVelocity = 1;

        /// <summary>
        /// Range of values controlling the particle color and alpha. Values for
        /// individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        [ContentSerializer]
        public Color MinColor = Color.White;

        /// <summary>
        /// Range of values controlling the particle color and alpha. Values for
        /// individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        [ContentSerializer]
        public Color MaxColor = Color.White;

        /// <summary>
        /// Range of values controlling the particle color and alpha. Values for
        /// individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        [ContentSerializer]
        public Vector4 MinColorVect;

        /// <summary>
        /// Range of values controlling the particle color and alpha. Values for
        /// individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        [ContentSerializer]
        public Vector4 MaxColorVect;

        /// <summary>
        /// Range of values controlling how fast the particles rotate. Values for
        /// individual particles are randomly chosen from somewhere between these
        /// limits. If both these values are set to 0, the particle system will
        /// automatically switch to an alternative shader technique that does not
        /// support rotation, and thus requires significantly less GPU power. This
        /// means if you don't need the rotation effect, you may get a performance
        /// boost from leaving these values at 0.
        /// </summary>
        [ContentSerializer]
        public float MinRotateSpeed = 0;

        /// <summary>
        /// Range of values controlling how fast the particles rotate. Values for
        /// individual particles are randomly chosen from somewhere between these
        /// limits. If both these values are set to 0, the particle system will
        /// automatically switch to an alternative shader technique that does not
        /// support rotation, and thus requires significantly less GPU power. This
        /// means if you don't need the rotation effect, you may get a performance
        /// boost from leaving these values at 0.
        /// </summary>
        [ContentSerializer]
        public float MaxRotateSpeed = 0;

        /// <summary>
        /// Range of values controlling how fast the particles rotate. Values for
        /// individual particles are randomly chosen from somewhere between these
        /// limits. If both these values are set to 0, the particle system will
        /// automatically switch to an alternative shader technique that does not
        /// support rotation, and thus requires significantly less GPU power. This
        /// means if you don't need the rotation effect, you may get a performance
        /// boost from leaving these values at 0.
        /// </summary>
        [ContentSerializer]
        public Vector2 RotateSpeed = Vector2.Zero;

        /// <summary>
        /// Range of values controlling how big the particles are when first created.
        /// Values for individual particles are randomly chosen from somewhere between
        /// these limits.
        /// </summary>
        [ContentSerializer]
        public float MinStartSize = 100;

        /// <summary>
        /// Range of values controlling how big the particles are when first created.
        /// Values for individual particles are randomly chosen from somewhere between
        /// these limits.
        /// </summary>
        [ContentSerializer]
        public float MaxStartSize = 100;

        /// <summary>
        /// Range of values controlling how big the particles are when first created.
        /// Values for individual particles are randomly chosen from somewhere between
        /// these limits.
        /// </summary>
        [ContentSerializer]
        public Vector2 StartSize = new Vector2(100, 100);

        /// <summary>
        /// Range of values controlling how big particles become at the end of their
        /// life. Values for individual particles are randomly chosen from somewhere
        /// between these limits.
        /// </summary>
        [ContentSerializer]
        public float MinEndSize = 100;

        /// <summary>
        /// Range of values controlling how big particles become at the end of their
        /// life. Values for individual particles are randomly chosen from somewhere
        /// between these limits.
        /// </summary>
        [ContentSerializer]
        public float MaxEndSize = 100;

        /// <summary>
        /// Range of values controlling how big particles become at the end of their
        /// life. Values for individual particles are randomly chosen from somewhere
        /// between these limits.
        /// </summary>
        [ContentSerializer]
        public Vector2 EndSize = new Vector2(100, 100);

        /// <summary>
        /// Alpha blending settings.
        /// </summary>
        [ContentSerializer]
        public Blend SourceBlend = Blend.SourceAlpha;

        /// <summary>
        /// Alpha blending settings.
        /// </summary>
        [ContentSerializer]
        public Blend DestinationBlend = Blend.InverseSourceAlpha;
    }

    public class SkyComponentDefinition
    {
        // Currently this component has no variables
    }

    public class CollisionTriggerComponentDefinition
    {
        /// <summary>
        /// Path to the CollisionEffects definition XML file.
        /// CollisionEffects are optional.
        /// </summary>
        [ContentSerializer]
        public string EffectsDefinitionXML;
    }

    public class TerrainComponentDefinition
    {
        /// <summary>
        /// The path to the image that will be used at the heightmap.
        /// This image must be power-of-two in height and width. (e.g. 512x512, 1024x1024)
        /// </summary>
        [ContentSerializer]
        public string HeightMapImagePath;

        /// <summary>
        /// The scale of the terrain from the heightmap dimensions. Using a scale of 1 means
        /// no scaling will occur, but for example if the heightmap was 512x512 and you choose
        /// a ScaleFactor of 2, the final terrain will be 1024x1024 in size.
        /// </summary>
        [ContentSerializer]
        public int ScaleFactor;

        /// <summary>
        /// Number of smoothing passes that will occur after the original heightmap is loaded.
        /// </summary>        
        [ContentSerializer]
        public int SmoothingPasses;

        /// <summary>
        /// How much difference in height between the lowest and highest parts of the heightmap.
        /// </summary>
        [ContentSerializer]
        public float ElevationStrength;
    }

    public class TerrainPhysicsComponentDefinition
    {
        // Currently this component has no variables
    }
}
