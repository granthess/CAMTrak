//
// SkyComponent.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;

using QuickStart;
using QuickStart.Entities;
using QuickStart.Interfaces;

namespace QuickStart.Components
{
    public abstract class SkyComponent : BaseComponent
    {
        protected SkyComponent(BaseEntity parent)
            : base(parent)
        {

        }

        /// <summary>
        /// Sent to a component when its parent entity is added to a scene
        /// </summary>
        /// <param name="manager">The <see cref="SceneManager"/> that added the entity to the scene.</param>
        public override void AddedToScene(SceneManager manager)
        {            
            // First we need to get the current rendering camera.
            MsgGetRenderEntity camMsg = ObjectPool.Aquire<MsgGetRenderEntity>();
            this.parentEntity.Game.SendInterfaceMessage(camMsg, InterfaceType.Camera);

            // If the ID we received is not empty, then we're good to go
            if (camMsg.EntityID != QSGame.UniqueIDEmpty)
            {
                MsgAttachSkyToCamera attachCamMsg = ObjectPool.Aquire<MsgAttachSkyToCamera>();
                attachCamMsg.CameraEntityID = camMsg.EntityID;
                attachCamMsg.UniqueTarget = this.parentEntity.UniqueID;
                this.parentEntity.Game.SendMessage(attachCamMsg);
            }
        }
    }
}
