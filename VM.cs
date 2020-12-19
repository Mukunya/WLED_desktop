using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VBase;
using MaterialDesignThemes.Wpf;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Net;

namespace WLED_desktop
{
    public class VM:ViewModel
    {
        public ObservableCollection<Light> Lights { get; set; }
        public VM()
        {
            Lights = new ObservableCollection<Light>();
            OpenDialog = new RelayCommand(o => DialogOpen = true);
            CloseDialog = new RelayCommand(o =>
            {
                DialogOpen = false;
                Properties.Settings.Default.Lights_JSON = JsonConvert.SerializeObject(Lights.ToArray());
                Properties.Settings.Default.Save();
            }
            
            );
            NewLight = new RelayCommand(o =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    Lights.Add(new Light() { Name = "New light" });
                    Item_SelectedChanged("New light", true);
                    Lights[Lights.Count-1].SelectedChanged += Item_SelectedChanged;
                    Lights[Lights.Count - 1].DoubleClicked += Item_DoubleClicked;
                    DialogOpen = true;
                });
            });
            ShowWindow = new RelayCommand(o =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    App.Current.MainWindow.Show();
                });
                
            });
            Light[] lights = JsonConvert.DeserializeObject<Light[]>(Properties.Settings.Default.Lights_JSON);
            foreach (var item in lights)
            {
                Lights.Add(item);
                item.SelectedChanged += Item_SelectedChanged;
                item.DoubleClicked += Item_DoubleClicked;
            }
            Timer.RunEach(15000, () =>
            {
                for (int i = 0; i < Lights.Count; i++)
                {
                    WebClient client = new WebClient();
                    string data = client.DownloadString("http://"+Lights[i].Address + "/json/state");
                    state s = JsonConvert.DeserializeObject<state>(data);
                    Lights[i].IsOn = s.on;
                }
            });
        }

        private void Item_DoubleClicked(object sender, EventArgs e)
        {
            DialogOpen = true;
        }

        public Light SelectedLight
        {
            get
            {
                if (Lights.Count>0)
                {
                    return Lights.First(o => o.Selected);
                }
                else
                {
                    return new Light();
                }
            }
        }
        public string WebSource
        {
            get
            {
                return SelectedLight.Address;
            }
        }
        private void Item_SelectedChanged(object sender, bool e)
        {
            int selectedindex = Lights.IndexOf(Lights.First(o => o.Name == (sender as string)));
            for (int i = 0; i < Lights.Count; i++)
            {
                if (i == selectedindex)
                {
                    Lights[i].Selected = true;
                }
                else
                {
                    Lights[i].Selected = false;
                }
            }
            OnPropertyChanged("SelectedLight");
            OnPropertyChanged("WebSource");
        }

        private bool dialogopen = false;
        public bool DialogOpen
        {
            get => dialogopen;
            set
            {
                dialogopen = value;
                OnPropertyChanged("DialogOpen");
            }
        }
        public ICommand OpenDialog { get; set; }
        public ICommand NewLight { get; set; }
        public ICommand CloseDialog { get; set; }
        public ICommand ShowWindow { get; set; }
    }

    public class Light : ViewModel
    {
        private string _address = "";
        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged("Address");
            }
        }

        private string _name = "Teszt";
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        private bool selected = false;

        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                OnPropertyChanged("Selected");
                OnPropertyChanged("CMode");
            }
        }

        public ColorZoneMode CMode
        {
            get
            {
                if (Selected)
                {
                    return ColorZoneMode.PrimaryDark;
                }
                else
                {
                    return ColorZoneMode.Dark;
                }
            }
        }
        public ICommand OpenDialog { get; set; }
        public ICommand Click
        {
            get;set;
        }

        private bool ison = true;

        public bool IsOn
        {
            get { return ison; }
            set 
            {
                ison = value;
                OnPropertyChanged("IsOn");
                WebClient client = new WebClient();
                string n = "";
                if (ison)
                {
                    n = "1";
                }
                else
                {
                    n = "0";
                }
                client.DownloadString("http://" + Address + "/win&T=" + n);
            }
        }
        private PackIconKind _icon = PackIconKind.Lightbulb;
        public PackIconKind Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                OnPropertyChanged("Icon");
            }
        }
        public event EventHandler<bool> SelectedChanged;
        public event EventHandler DoubleClicked;
        public Light() 
        {
            Click = new RelayCommand(o =>
            {
                Selected = !Selected;
                SelectedChanged?.Invoke(Name,Selected);
            });
            OpenDialog = new RelayCommand(o => DoubleClicked?.Invoke(null, null));
        }
    }
}

public struct state
{
    public bool on;
}