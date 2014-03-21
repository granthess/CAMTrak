//
// SkyDomeComponent.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using QuickStart;
using QuickStart.Entities;

namespace QuickStart.Components
{
    public class SkyDomeComponent : SkyComponent
    {
        public override ComponentType GetComponentType() { return ComponentType.SkyDomeComponent; }

        public static BaseComponent LoadFromDefinition(ContentManager content, string definitionPath, BaseEntity parent)
        {
            SkyComponentDefinition compDef = content.Load<SkyComponentDefinition>(definitionPath);

            SkyDomeComponent newComponent = new SkyDomeComponent(parent, compDef);
            return newComponent;
        }

        /// <summary>
        /// This is the ID of the Entity that the SkyDome attaches to. 
        /// This keeps the skydome always rendering at the same distance.
        /// </summary>
        private Int64 CameraEntityID;

        public SkyDomeComponent(BaseEntity parent)
            : base(parent)
        {
            ActivateComponent();
        }

        public SkyDomeComponent(BaseEntity parent, SkyComponentDefinition compDef)
            : base(parent)
        {
            ActivateComponent();
        }

        /// <summary>
        /// Update the component
        /// </summary>
        /// <param name="gameTime">XNA Timing snapshot</param>
        public override void Update(GameTime gameTime)
        {
            if (CameraEntityID != QSGame.UniqueIDEmpty)
            {
                MsgGetPosition msgGetPos = ObjectPool.Aquire<MsgGetPosition>();
                msgGetPos.UniqueTarget = CameraEntityID;

                this.parentEntity.Game.SendMessage(msgGetPos);

                // @HACK : I'm using the skydome model file I found on Riemer Grootjans's site, because I'm too
                //         lazy to use 3DSMax to lower my skydome model, I'm lowering it myself. If you make your
                //         own custom skydome model, please remove these two lines. :) - N.Foster
                Vector3 pos = msgGetPos.Position;
                pos.Y -= 600f;

                MsgSetPosition msgSetPos = ObjectPool.Aquire<MsgSetPosition>();
                msgSetPos.Position = pos;
                msgSetPos.Type = MessageType.SetPosition;
                msgSetPos.UniqueTarget = this.parentEntity.UniqueID;
                this.parentEntity.Game.SendMessage(msgSetPos);
            }
        }

        /// <summary>
        /// Initializes this component.
        /// </summary>
        public override void Initialize()
        {
            this.parentEntity.AddComponent(this);
        }

        /// <summary>
        /// Shutdown this component
        /// </summary>
        public override void Shutdown()
        {
        }

        /// <summary>
        /// Handles a message sent to this component.
        /// </summary>
        /// <param name="message">Message to be handled</param>
        /// <returns>True, if handled, otherwise false</returns>
        /// <exception cref="ArgumentException">Thrown if a <see cref="MessageType"/> is not handled properly."/></exception>
        public override bool ExecuteMessage(IMessage message)
        {
            if (message.UniqueTarget != this.parentEntity.UniqueID)
                return false;

            switch (message.Type)
            {
                case MessageType.SkyAttachToCamera:
                    {
                        MsgAttachSkyToCamera attachMsg = message as MsgAttachSkyToCamera;
                        message.TypeCheck(attachMsg);

                        CameraEntityID = attachMsg.CameraEntityID;
                    }
                    return true;
                default:
                    return false;
            }
        }
    }
}