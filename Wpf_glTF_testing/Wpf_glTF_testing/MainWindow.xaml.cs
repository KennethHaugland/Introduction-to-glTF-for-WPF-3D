using glTFLoader.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wpf_glTF_testing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public AxisAngleRotation3D rot_x { get; set; }
        public AxisAngleRotation3D rot_y { get; set; }
        public AxisAngleRotation3D rot_z { get; set; }
        public ScaleTransform3D zoom { get; set; } = new ScaleTransform3D(0.1d,0.1d,0.1d);
        public TranslateTransform3D center { get; set; }


        public string[] planets = { 
                // 0
                "Sun_1_1391000.glb", 
                // 1
                "Mercury_1_4878.glb", 
                // 2
                "Venus_1_12103.glb", 
                // 3
                "Earth_1_12756.glb",
                // 4
                "Mars_1_6792.glb",
                // 5
                "Jupiter_1_142984.glb",
                // 6
                "Saturn_1_120536.glb",
                // 7
                "Uranus_1_51118.glb",
                // 8
                "Neptune_1_49528.glb",
                // 9
                "Pluto_1_2374.glb"};
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

          
            PlanetSelect.ItemsSource = planets;
            var TheCurrentPlanet = new Planet(planets[0]);
            viewport3D1.Children.Add(TheCurrentPlanet.Visualisation);
            
            center = new TranslateTransform3D(0, 0, 0);
            rot_x = new AxisAngleRotation3D(
                new Vector3D(1, 0, 0),
                slider_x.Value);
            rot_y = new AxisAngleRotation3D(
                new Vector3D(0, 1, 0),
                slider_y.Value);
            rot_z = new AxisAngleRotation3D(
                new Vector3D(0, 0, 1),
                slider_z.Value);
            zoom = new ScaleTransform3D(
                slider_zoom.Value,
                slider_zoom.Value,
                slider_zoom.Value);

            Transform3DGroup t = new Transform3DGroup();

            t.Children.Add(zoom);

            // the order of the following three is significant
            t.Children.Add(new RotateTransform3D(rot_y));
            t.Children.Add(new RotateTransform3D(rot_x));
            t.Children.Add(new RotateTransform3D(rot_z));

            t.Children.Add(center);

            viewport3D1.Camera.Transform = t;
        }

        void slider_x_changed(object sender, RoutedEventArgs args)
        {
            rot_x.Angle = slider_x.Value;
        }
        void slider_y_changed(object sender, RoutedEventArgs args)
        {
            rot_y.Angle = slider_y.Value;
        }
        void slider_z_changed(object sender, RoutedEventArgs args)
        {
            rot_z.Angle = slider_z.Value;
        }

        void slider_zoom_changed(object sender, RoutedEventArgs args)
        {
                zoom.ScaleY = slider_zoom.Value;
                zoom.ScaleX = slider_zoom.Value;
                zoom.ScaleZ = slider_zoom.Value;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = PlanetSelect.SelectedIndex;
            var gg = new Planet(planets[index]);

            viewport3D1.Children.Clear();
            viewport3D1.InvalidateVisual();
            DirectionalLight myDirectionalLight = new DirectionalLight();
            myDirectionalLight.Color = Colors.White;
            myDirectionalLight.Direction = new Vector3D(-1, -1, -1);
            viewport3D1.Children.Add(new ModelVisual3D() { Content = myDirectionalLight });
            viewport3D1.Children.Add(gg.Visualisation);
            viewport3D1.InvalidateVisual();
        }
    }
}
