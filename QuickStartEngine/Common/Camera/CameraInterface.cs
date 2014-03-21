//
// CameraInterface.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using QuickStart.Entities;
using QuickStart.Geometry;
using QuickStart.Interfaces;
using QuickStart.Mathmatics;

namespace QuickStart.Camera
{
    /// <summary>
    /// All camera messages will be handled by the camera interface. It will allow
    /// anyone in the code that has access to this interface to request camera information.
    /// </summary>
    public class CameraInterface : QSInterface
    {
        /// <summary>
        /// This is the current render camera for the game.
        /// </summary>
        /// <remarks>
        /// Upon creating split-screen support we will need multiple render cameras. We'd need at
        /// least one camera per viewport.
        /// </remarks>
        public BaseEntity RenderCamera
        {
            get { return renderCamera; }
        }
        private BaseEntity renderCamera;

        /// <summary>
        /// Create a <see cref="CameraInterface"/>.
        /// </summary>
        public CameraInterface(QSGame game)
            : base(game, InterfaceType.Camera)
        {
            this.game.GameMessage += this.Game_GameMessage;
        }

        public override void Shutdown()
        {
            renderCamera = null;
        }

        /// <summary>
        /// Sets the camera for which rendering will occur.
        /// </summary>
        /// <param name="inCamera">Desired camera to render from</param>        
        private void SetRenderCamera(BaseEntity inCamera)
        {
            this.renderCamera = inCamera;
        }

        /// <summary>
        /// Calculates a LineSegment from the camera's position to the
        /// 3D position of the cursor, as if it were on the camera's farplane.
        /// </summary>        
        public void CalculateCursorLineSegment(out LineSegment segment)
        {
            segment.start = Vector3.Zero;
            segment.end = Vector3.Zero;

            if (null == this.renderCamera)
            {                
                return;
            }

            // Get position from Input Interface
            InputInterface input = this.game.SceneManager.GetInterface(InterfaceType.Input) as InputInterface;
            if (null == input)
            {
                throw new Exception("Input interface was not initialized before call to CalculateCursorRay()");
            }

            CursorInfo info = input.GetCursorInfo();

            if (!info.IsVisible)
            {
                // Cursor is not visible, so we just bail
                return;
            }

            // create 2 positions in screenspace using the cursor position. 0 is as
            // close as possible to the camera, 1 is as far away as possible.
            Vector3 nearSource = new Vector3(info.Position, 0f);
            Vector3 farSource = new Vector3(info.Position, 1f);

            MsgGetViewport msgGetPort = ObjectPool.Aquire<MsgGetViewport>();
            this.game.SendInterfaceMessage(msgGetPort, InterfaceType.Graphics);            

            MsgGetProjectionMatrix msgGetProj = ObjectPool.Aquire<MsgGetProjectionMatrix>();
            msgGetProj.UniqueTarget = renderCamera.UniqueID;
            this.game.SendMessage(msgGetProj);            

            MsgGetViewMatrix msgGetView = ObjectPool.Aquire<MsgGetViewMatrix>();
            msgGetView.UniqueTarget = renderCamera.UniqueID;
            this.game.SendMessage(msgGetView);

            Viewport vPort = msgGetPort.Data;
            Matrix projMat = msgGetProj.Data;
            Matrix viewMat = msgGetView.Data;

            // use Viewport.Unproject to tell what those two screen space positions
            // would be in world space. we'll need the projection matrix and view
            // matrix, which we have saved as member variables. We also need a world
            // matrix, which can just be identity.
            segment.start = vPort.Unproject(nearSource, projMat, viewMat, Matrix.Identity);
            segment.end = vPort.Unproject(farSource, projMat, viewMat, Matrix.Identity);
        }

        /// <summary>
        /// Message listener for messages that are not directed at any particular Entity or Interface.
        /// </summary>
        /// <param name="message">Incoming message</param>
        /// <exception cref="ArgumentException">Thrown if a <see cref="MessageType"/> is not handled properly."/></exception>
        protected virtual void Game_GameMessage(IMessage message)
        {
            ExecuteMessage(message);
        }

        /// <summary>
        /// Message handler for all incoming messages.
        /// </summary>
        /// <param name="message">Incoming message</param>
        /// <remarks>If a message is sent to the interface and not handled by
        /// the interface then it will be forwarded to the current render camera for
        /// handling.</remarks>
        /// <exception cref="ArgumentException">Thrown if a <see cref="MessageType"/> is not handled properly."/></exception>
        public override bool ExecuteMessage(IMessage message)
        { 
            switch (message.Type)
            {
                case MessageType.CameraSetRender:
                    {
                        MsgSetRenderEntity camMsg = message as MsgSetRenderEntity;
                        message.TypeCheck(camMsg);

                        SetRenderCamera(camMsg.Entity);
                    }
                    return true;
                case MessageType.CameraGetRenderEntityID:
                    {
                        MsgGetRenderEntity camMsg = message as MsgGetRenderEntity;
                        message.TypeCheck(camMsg);                        

                        if (null != renderCamera)
                        {
                            camMsg.EntityID = renderCamera.UniqueID;
                        }
                    }
                    return true;
                case MessageType.GraphicsSettingsChanged:
                    {
                        // If we have a render camera, let them know about the change to graphics settings.
                        if (null != renderCamera)
                        {
                            MsgGraphicsSettingsChanged msgChange = ObjectPool.Aquire<MsgGraphicsSettingsChanged>();
                            msgChange.UniqueTarget = renderCamera.UniqueID;
                            this.game.SendMessage(msgChange);
                        }
                    }
                    return true;
                case MessageType.GetLineSegmentToCursor:
                    {
                        MsgGetLineSegmentToCursor msgGetRay = message as MsgGetLineSegmentToCursor;
                        message.TypeCheck(msgGetRay);

                        if (null != renderCamera)
                        {
                            LineSegment segment;
                            CalculateCursorLineSegment(out segment);
                            msgGetRay.lineSegment = segment;
                        }
                    }
                    return true;
                default:
                    return false;
            }
        }
    }
}
