﻿using MP.Dataplane;
using MP.db;
using MP.Properties;
using MP.ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MP.ExpermentsResults.Energy_consumptions
{
    /// <summary>
    /// Interaction logic for UiMergedPathVsIndpendentPath.xaml
    /// </summary>
    public partial class UiMergedPathVsIndpendentPath : Window
    {
        public DispatcherTimer TodoTimer = new DispatcherTimer();
        private bool AreSinksIntialized = false;
        public MainWindow mainWindow { set; get; }
        public UiMergedPathVsIndpendentPath(MainWindow _MainWindow) 
        {
            InitializeComponent();
            mainWindow = _MainWindow;
            ImportNetwork();

        }


        /// <summary>
        /// you can run this one.
        /// </summary>
        public void StartSimulation()
        {
            LoadSettings();

            Run();
        }

        // 1- laod
        //-2 Run



        private void Run()
        {
            string netName = com_netName.Text;
            if (netName != "")
            {

                bool isOK = SetParamaters(); // set paramaters
                if (isOK)
                {
                    Deploy(netName);
                    mainWindow.RandomDeplayment();
                    // activate todo timer:
                    TodoTimer.Interval = TimeSpan.FromSeconds(1);
                    TodoTimer.Tick += TodoTimer_Tick;
                    TodoTimer.Start();
                    this.Close();
                }
            }
        }




        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Run();
        }

        /// <summary>
        /// make sure that the user has selected all  paramaters.
        /// </summary>
        /// <returns></returns>
        private bool SetParamaters()
        {
            try
            {
                
                Settings.Default.MaxClusteringThreshold = Convert.ToDouble(com_cluserting_angle.Text);
                Settings.Default.ActivePeriod= Convert.ToDouble(comb_active.Text);
                Settings.Default.SleepPeriod= Convert.ToDouble(comb_sleep.Text);
                Settings.Default.MacStartUp= Convert.ToInt32(comb_startup.Text);
                Settings.Default.BatteryIntialEnergy= Convert.ToDouble(com_intial_Energy.Text);
                Settings.Default.QueueTime = Convert.ToDouble(com_queueTime.Text);
                Settings.Default.MaxNumberofObtianPackets = Convert.ToInt32(com_number_of_packets.Text);
                Settings.Default.CoordinationType = comb_coordination.Text;

                if (Settings.Default.StopeWhenFirstNodeDeid)
                {
                    Settings.Default.StopSimlationWhen = 1000000000;
                }
                else
                {
                    Settings.Default.StopSimlationWhen = Convert.ToUInt32(com_simutime.Text);
                }

                Settings.Default.Save();
                return true;
            }
            catch
            {
                MessageBox.Show("Please set the paramaters first!");
                return false;
            }
        }


        /// <summary>
        /// run the todo timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TodoTimer_Tick(object sender, EventArgs e)
        {

            // intialize the sinks after MacStartUp.
            if (PublicParamerters.SimulationTime >= PublicParamerters.MacStartUp)
            {
                if (!AreSinksIntialized)
                {
                    InstallSinks();
                    AreSinksIntialized = true; //
                }
            }

            // activate the packet rate, and then close the timer imediatly
            if (PublicParamerters.SimulationTime >= PublicParamerters.MacStartUp + 5)
            {
                if (chk_fixedNumber_of_packets.IsChecked == false)
                {
                    Settings.Default.PacketRate = Convert.ToDouble(com_packet_rate.Text);
                    Settings.Default.Save();

                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    mainWindow.SetPacketRateS1(Settings.Default.PacketRate);
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));

                    mainWindow.SetNumberOfPackets(Settings.Default.MaxNumberofObtianPackets);
                }


                // stop todo timer:
                TodoTimer.Interval = TimeSpan.FromSeconds(0);
                TodoTimer.Stop();

            }
        }

        /// <summary>
        /// install the sinks
        /// </summary>
        private void InstallSinks()
        {
            string sikCouStr = com_sinksNumber.Text;
            if (sikCouStr != "")
            {
                int sinksCount = Convert.ToInt32(sikCouStr);
                Settings.Default.SinkCount = sinksCount;
                Settings.Default.Save();
                if (sinksCount >= 0)
                {
                    for (int i = 0; i < sinksCount; i++)
                    {
                        Thread.Sleep(i * 100);
                        Sink sink = new Sink();
                        mainWindow.Canvas_SensingFeild.Children.Add(sink);
                    }
                }
            }
        }

        /// <summary>
        /// deplay the nodes
        /// </summary>
        /// <param name="netname"></param>
        private void Deploy(string netname)
        {
            NetwokImport im = new NetwokImport();
            im.MainWindow = mainWindow;
            im.ImportedSensorSensors = NetworkTopolgy.ImportNetwok(netname);
            im.Deploy(netname);
        }

        /// <summary>
        /// import the network
        /// </summary>
        private void ImportNetwork()
        {
            List<string> netNames = NetworkTopolgy.ImportNetworkNamesAsStrings();
            foreach (string name in netNames)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem() { Content = name };
                com_netName.Items.Add(comboBoxItem);
            }
        }

        private void LoadSettings()
        {
            com_netName.Text = Settings.Default.NetworkName;
            com_cluserting_angle.Text = Settings.Default.MaxClusteringThreshold.ToString();
            com_packet_rate.Text = Settings.Default.PacketRate.ToString();
            com_simutime.Text = Settings.Default.StopSimlationWhen.ToString();
            com_sinksNumber.Text = Settings.Default.SinkCount.ToString();

            comb_coordination.Text = Settings.Default.CoordinationType;

            comb_active.Text = Settings.Default.ActivePeriod.ToString();
            comb_sleep.Text = Settings.Default.SleepPeriod.ToString();
            comb_startup.Text = Settings.Default.MacStartUp.ToString();
            com_intial_Energy.Text = Settings.Default.BatteryIntialEnergy.ToString();
            com_queueTime.Text = Settings.Default.QueueTime.ToString();


            chk_ismobile.IsChecked = Settings.Default.IsMobileSink;
            chk_stope_when_first_node_deis.IsChecked = Settings.Default.StopeWhenFirstNodeDeid;

            chk_save_logs.IsChecked = Settings.Default.SaveRoutingLog;
            chk_drawrouts.IsChecked = Settings.Default.ShowRoutingPaths;

            chek_show_radar.IsChecked = Settings.Default.ShowRadar;

            chek_animation.IsChecked = Settings.Default.ShowAnimation;

            chk_save_packets.IsChecked = Settings.Default.SavePackets;

            chk_consider_energy.IsChecked = Settings.Default.ConsiderEnergy;


            com_number_of_packets.Text = Settings.Default.MaxNumberofObtianPackets.ToString();
            chk_fixedNumber_of_packets.IsChecked = Settings.Default.FixNumberofPck;
            ck_SinksStartAtNetworkCenter.IsChecked = Settings.Default.SinksStartAtNetworkCenter;

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        private void chk_ismobile_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.IsMobileSink = true;
            Settings.Default.Save();
        }

        private void chk_ismobile_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.IsMobileSink = false;
            Settings.Default.Save();
        }






        private void chk_stope_when_first_node_deis_Checked(object sender, RoutedEventArgs e)
        {
            com_simutime.IsEnabled = false;
            Settings.Default.StopeWhenFirstNodeDeid = true;
            Settings.Default.StopSimlationWhen = 1000000000;
            Settings.Default.Save();

        }

        private void chk_stope_when_first_node_deis_Unchecked(object sender, RoutedEventArgs e)
        {
            com_simutime.IsEnabled = true;
            Settings.Default.StopeWhenFirstNodeDeid = false;
            Settings.Default.Save();
        }



        private void chk_drawrouts_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowRoutingPaths = true;
            Settings.Default.Save();
        }

        private void chk_drawrouts_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowRoutingPaths = false;
            Settings.Default.Save();
        }

        private void chk_save_logs_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.SaveRoutingLog = true;
            Settings.Default.Save();
        }

        private void chk_save_logs_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.SaveRoutingLog = false;
            Settings.Default.Save();
        }

        private void chek_show_radar_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowRadar = true;
            Settings.Default.Save();
        }

        private void chek_show_radar_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowRadar = false;
            Settings.Default.Save();
        }

        private void chek_animation_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowAnimation = true;
            Settings.Default.AnimationSpeed = 0.1;
            Settings.Default.Save();
        }

        private void chek_animation_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowAnimation = false;
            Settings.Default.AnimationSpeed = 0;
            Settings.Default.Save();
        }

        private void chk_save_packets_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.SavePackets = true;
            Settings.Default.Save();
        }

        private void chk_save_packets_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.SavePackets = false;
            Settings.Default.Save();
        }

        private void chk_consider_energy_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ConsiderEnergy = true;
            Settings.Default.Save();
        }

        private void chk_consider_energy_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ConsiderEnergy = false;
            Settings.Default.Save();
        }

        private void chk_fixedNumber_of_packets_Checked(object sender, RoutedEventArgs e)
        {
            com_packet_rate.IsEnabled = false;
            chk_stope_when_first_node_deis.IsChecked = true;
            chk_stope_when_first_node_deis.IsEnabled = false;
            com_number_of_packets.IsEnabled = true;
            Settings.Default.FixNumberofPck = true;
            Settings.Default.Save();
        }

        private void chk_fixedNumber_of_packets_Unchecked(object sender, RoutedEventArgs e)
        {
            com_packet_rate.IsEnabled = true;
            chk_stope_when_first_node_deis.IsEnabled = true;
            com_number_of_packets.IsEnabled = false;
            Settings.Default.FixNumberofPck = false;
            Settings.Default.Save();
        }

        private void ck_SinksStartAtNetworkCenter_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.SinksStartAtNetworkCenter = false;
            Settings.Default.Save();
        }

        private void ck_SinksStartAtNetworkCenter_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.SinksStartAtNetworkCenter = true;
            Settings.Default.Save();
        }
    }
}
