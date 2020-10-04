using MP.Dataplane;
using MP.Dataplane.NOS;
using System.Collections.Generic;
using System.Windows;

namespace MP.ui
{
    /// <summary>
    /// Interaction logic for UiRecievedPackertsBySink.xaml
    /// </summary>
    public partial class UiRecievedPackertsBySink : Window
    {

        public UiRecievedPackertsBySink(List<Packet> packets)
        {
            InitializeComponent();


            List<Packet> copy = new List<Packet>();
            copy.AddRange(packets);
            dg_packets.ItemsSource = copy;
        }
    }
    
}
