//
// BaseInputComponent.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using QuickStart;
using QuickStart.Entities;
using QuickStart.Input;

namespace QuickStart.Components
{
    abstract public class BaseInputComponent : BaseComponent
    {
        /// <summary>
        /// Listens to input messages, and stores values for polling
        /// </summary>
        protected InputPollingHandler inputs;

        /// <summary>
        /// Holds true/false whether the camera controls are inverted or not.
        /// </summary>
        public bool Inverted
        {
            get { return inverted; }
            set { inverted = value; }
        }
        protected bool inverted = false;

        /// <summary>
        /// Modifier for left thumb stick sensitivity for this input component.
        /// </summary>
        public float LeftThumbStickModifier
        {
            get { return leftThumbStickModifier; }
            set { leftThumbStickModifier = value; }
        }
        protected float leftThumbStickModifier = 2.0f;

        /// <summary>
        /// Modifier for right thumb stick sensitivity for this input component.
        /// </summary>
        public float RightThumbStickModifier
        {
            get { return rightThumbStickModifier; }
            set { rightThumbStickModifier = value; }
        }
        protected float rightThumbStickModifier = 0.02f;

        /// <summary>
        /// Called from a child class whenever an input component is created.
        /// </summary>
        /// <param name="parent">Parent entity of this component</param>
        public BaseInputComponent(BaseEntity parent)
            : base(parent)
        {
        }
    }
}
