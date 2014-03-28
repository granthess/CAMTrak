using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Xna.Framework;
using theprof.CAMTrak.XNAView;
using QuickStart.Physics;
using QuickStart.Interfaces;

namespace theprof.CAMTrak
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        EditorWindow editor_game;
        MapWindow map_game;
        

        public MainWindow()
        {
            InitializeComponent();

            editor_game = new EditorWindow(xnaControl1.Handle, "Content");
            map_game = new MapWindow(xnaControl2.Handle, "Content");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PhysicsInterface editor_physics = this.editor_game.SceneManager.GetInterface(InterfaceType.Physics) as PhysicsInterface;
            editor_physics.Shutdown();

            PhysicsInterface map_physics = this.map_game.SceneManager.GetInterface(InterfaceType.Physics) as PhysicsInterface;
            map_physics.Shutdown();

            editor_game.Exit();
            map_game.Exit();
        }
    }
}
