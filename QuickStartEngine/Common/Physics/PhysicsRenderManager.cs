// PhysicsRenderManager.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using QuickStart;
using QuickStart.Entities;
using QuickStart.Interfaces;
using QuickStart.Mathmatics;

using BEPUphysicsDrawer.Models;

namespace QuickStart.Physics
{
    public class PhysicsRenderManager : BaseManager
    {
        public ModelDrawer modelDrawer;

        public PhysicsRenderManager(QSGame game)
            : base(game)
        {
            // By default physics rendering is disabled.
            this.Enabled = false;

            this.Game.Services.AddService(typeof(PhysicsRenderManager), this);

            this.modelDrawer = new InstancedModelDrawer(game);
            this.modelDrawer.IsWireframe = true;
        }

        protected override void InitializeCore()
        {            
        }

        protected override void UpdateCore( GameTime gameTime )
        {
            if (this.Enabled)
            {
                this.modelDrawer.Update();
            }
        }

        public void Draw(GameTime gameTime)
        {
            if (!this.Enabled)
            {
                return;
            }

            // First we need to get the current rendering camera.
            var camMsg = ObjectPool.Aquire<MsgGetRenderEntity>();
            this.Game.SendInterfaceMessage(camMsg, InterfaceType.Camera);

            // Make sure the camera's unique ID is valid
            if (camMsg.EntityID != QSGame.UniqueIDEmpty)
            {
                // Get view matrix
                var msgGetViewMatrix = ObjectPool.Aquire<MsgGetViewMatrix>();
                msgGetViewMatrix.UniqueTarget = camMsg.EntityID;
                this.Game.SendMessage(msgGetViewMatrix);

                // Get projection matrix
                var msgGetProjMatrix = ObjectPool.Aquire<MsgGetProjectionMatrix>();
                msgGetProjMatrix.UniqueTarget = camMsg.EntityID;
                this.Game.SendMessage(msgGetProjMatrix);

                this.modelDrawer.Draw(msgGetViewMatrix.Data, msgGetProjMatrix.Data);
            }
        }
    }
}
