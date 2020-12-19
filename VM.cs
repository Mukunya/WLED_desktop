using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VBase;
using MaterialDesignThemes.Wpf;
using System.Windows.Input;

namespace WLED_desktop
{
    public class VM:ViewModel
    {
        public ObservableCollection<Light> Lights { get; set; }
        public VM()
        {
            Lights = new ObservableCollection<Light>();
            OpenDialog = new RelayCommand(o => DialogOpen = true);
            CloseDialog = new RelayCommand(o => DialogOpen = false);
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
            Lights.Add(new Light() { Name = "Teszt 1", IsOn = true, Selected=true });
            Lights.Add(new Light() { Name = "Teszt 2", IsOn = false });
            Lights.Add(new Light() { Name = "Teszt 3", IsOn = false });
            Lights.Add(new Light() { Name = "Teszt 4", IsOn = true });
            foreach (var item in Lights)
            {
                item.SelectedChanged += Item_SelectedChanged;
                item.DoubleClicked += Item_DoubleClicked;
            }
        }

        private void Item_DoubleClicked(object sender, EventArgs e)
        {
            DialogOpen = true;
        }

        public Light SelectedLight
        {
            get
            {
                return Lights.First(o => o.Selected);
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
