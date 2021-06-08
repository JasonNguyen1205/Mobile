using Acr.UserDialogs;
using static Xamarin.Forms.PlatformConfiguration.Android;
using Mobile.Function;
using Mobile.Object;
using Mobile.Utils;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Debug = System.Diagnostics.Debug;
using IAdapter = Plugin.BLE.Abstractions.Contracts.IAdapter;

namespace Mobile.ViewModels
{
    public class LoadTempModel : BaseViewModel
    {
        // General
        private readonly IMvxNavigationService _navigation;
        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();
        public ICharacteristic Characteristic { get; private set; }


        public IDescriptor Descriptor { get; set; }
        public string CharacteristicValue => Characteristic?.Value.ToHexString().Replace("-", " ");

        private readonly IUserDialogs _userDialogs;
        public Core core = new Core();
        public bool _updatesStarted;

        public bool TempScanLock = true;
        public string TempStatusText => TempScanLock ? "Temp Scan" : "Temp Lock";

        BleMainInfo MainInfo = new BleMainInfo();

        // Page Device Info
        public BleSensorInfo bleSensorInfo = new BleSensorInfo();
        public BleStoveLock bleStoveLock = new BleStoveLock();

        // Communication Pair
        //
        BleTempScan bts = new BleTempScan();
        double[,] intensities;
        int count = 1;
        DataManager dataManager;
        ColorUtil colorUtil;

        //

        BleMoveData moveData = new BleMoveData();

        public string Permissions
        {
            get
            {
                if (Characteristic == null)
                    return string.Empty;

                return (Characteristic.CanRead ? "Read " : string.Empty) +
                       (Characteristic.CanWrite ? "Write " : string.Empty) +
                       (Characteristic.CanUpdate ? "Update" : string.Empty);
            }
        }

        public LoadTempModel(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;
        }

        public override async void Prepare(MvxBundle parameters)
        {
            base.Prepare(parameters);
            foreach (var connectedDevice in Adapter.ConnectedDevices)
            {
                //update rssi for already connected evices (so tha 0 is not shown in the list)
                try
                {
                    //essages.Insert(0, $"Prepare");
                    //await connectedDevice.UpdateRssiAsync();
                    if (connectedDevice != null)
                    {
                        var Service = await connectedDevice.GetServiceAsync(Guid.Parse("69ddd530-8216-480e-a48d-a516ae310fc2"));
                        Messages.Insert(0, connectedDevice.ToString());
                        if (Service != null)
                        {
                            Characteristic = await Service.GetCharacteristicAsync(Guid.Parse("d0962ce0-360e-4db6-a356-b443a20d94e3"));
                            Messages.Insert(0, Service.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    await _userDialogs.AlertAsync($"Failed to update RSSI for {connectedDevice.Name}");
                    Messages.Insert(0, $"Failed to update RSSI for {connectedDevice.Name}");
                }
            }
            //LoadCommunicationPair();
            //CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            //CancellationToken cancellationToken = cancellationTokenSource.Token;

            //await Task.Delay(1000).ContinueWith(async (t) =>
            //{
            //    //LoadDeviceInfo();
            //StartUpdate();
            //}, cancellationToken);
            //await Task.Run(()=> { LoadTemp(); });
            //await Task.Delay(1000).ContinueWith(async (t) =>
            //{
            //    //LoadDeviceInfo();
            //    //LoadMovement();

            //}, cancellationToken);

            //LoadTemp();

            //StartUpdate();
            //LoadTemp();
            //LoadMovement();
            //Debug.WriteLine("Prepare " +Characteristic);
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            if (Characteristic != null)
            {
                return;
            }

            var navigation = Mvx.IoCProvider.Resolve<IMvxNavigationService>();
            navigation.Close(this);
        }

        public override void ViewDisappeared()
        {
            base.ViewDisappeared();

            if (Characteristic != null)
            {
                StopUpdate();
            }

        }

        // public MvxCommand ReadCommand => new MvxCommand(ReadValueAsync);

        //private async void ReadValueAsync()
        //{
        //    if (Characteristic == null)
        //        return;

        //    try
        //    {
        //        _userDialogs.ShowLoading("Reading characteristic value...");

        //        await Characteristic.ReadAsync();

        //        await RaisePropertyChanged(() => CharacteristicValue);

        //        Messages.Insert(0, $"Read value {CharacteristicValue}");
        //    }
        //    catch (Exception ex)
        //    {
        //        _userDialogs.HideLoading();
        //        await _userDialogs.AlertAsync(ex.Message);

        //        Messages.Insert(0, $"Error {ex.Message}");

        //    }
        //    finally
        //    {
        //        _userDialogs.HideLoading();
        //    }

        //}

        public MvxCommand ToggleTempScan => new MvxCommand((() =>
        {
            if (TempScanLock)
            {
                TempScanLock = false;

                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = cancellationTokenSource.Token;
                LoadTemp();
                StartScan();
                RaisePropertyChanged(() => TempStatusText);

            }
            else
            {
                TempScanLock = true;
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = cancellationTokenSource.Token;
                //Task.Delay(1000).ContinueWith(async (t) =>
                //{
                StopScan();
                //}, cancellationToken);
                RaisePropertyChanged(() => TempStatusText);
            }

        }));


        public async void StartScan()
        {
            byte[] BLE_CMD_APP_STATE_SET = null;
            BleSetup bs = new BleSetup();
            bs.Enable = true;
            BLE_CMD_APP_STATE_SET = core.BleCommunication.Send(BleCommand.BLE_CMD_APP_STATE_SET, bs.GetBytes());
            await Characteristic.WriteAsync(BLE_CMD_APP_STATE_SET);

            byte[] BLE_CMD_TEMP_SCAN = null;
            bts.Enable = true;
            BLE_CMD_TEMP_SCAN = core.BleCommunication.Send(BleCommand.BLE_CMD_TEMP_SCAN, bts.GetBytes());
            await Characteristic.WriteAsync(BLE_CMD_TEMP_SCAN);

            await RaisePropertyChanged(() => CharacteristicValue);
            //Messages.Insert(0, $"Wrote value {CharacteristicValue}");          
        }

        public async void StopScan()
        {
            byte[] BLE_CMD_TEMP_SCAN = null;
            bts.Enable = false;
            BLE_CMD_TEMP_SCAN = core.BleCommunication.Send(BleCommand.BLE_CMD_TEMP_SCAN, bts.GetBytes());
            await Characteristic.WriteAsync(BLE_CMD_TEMP_SCAN);

            await RaisePropertyChanged(() => CharacteristicValue);
            //Messages.Insert(0, $"Wrote value {CharacteristicValue} Stop");
        }

        // TEMP SCAN 2
        public void LoadTemp()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            //Task.Delay(2000).ContinueWith((t) =>
            //{
            //    StartUpdate();
            //}, cancellationToken);
            new Thread(() =>
            {
                Device.InvokeOnMainThreadAsync(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(500).ContinueWith(async (t) => { await StartUpdate(); });
                    }
                });

            }).Start();

        }

        private async void WriteTempSync()
        {

            byte[] BLE_CMD_APP_STATE_SET = null;
            BleSetup bs = new BleSetup();
            bs.Enable = true;
            BLE_CMD_APP_STATE_SET = core.BleCommunication.Send(BleCommand.BLE_CMD_APP_STATE_SET, bs.GetBytes());
            await Characteristic.WriteAsync(BLE_CMD_APP_STATE_SET);

            //byte[] BLE_CMD_TEMP_SCAN = null;
            //bts.Enable = true;
            //BLE_CMD_TEMP_SCAN = core.BleCommunication.Send(BleCommand.BLE_CMD_TEMP_SCAN, bts.GetBytes());
            //await Characteristic.WriteAsync(BLE_CMD_TEMP_SCAN);


            await RaisePropertyChanged(() => CharacteristicValue);
            //Messages.Insert(0, $"Wrote value {CharacteristicValue} Write temp sync");
        }

        public async Task StartUpdate()
        {
            try
            {

                Characteristic.ValueUpdated -= CharacteristicOnValueUpdated;
                Characteristic.ValueUpdated += CharacteristicOnValueUpdated;

                await Characteristic.StartUpdatesAsync();
                //Messages.Insert(0, $"Start updates");
                //await RaisePropertyChanged(() => { StoveLock ? });
            }
            catch (Exception ex)
            {
                await _userDialogs.AlertAsync(ex.Message);
            }

        }

        private async void StopUpdate()
        {
            try
            {
                _updatesStarted = false;

                await Characteristic.StopUpdatesAsync();
                Characteristic.ValueUpdated -= CharacteristicOnValueUpdated;
                //Messages.Insert(0, $"Stop updates");

                //  await RaisePropertyChanged(() => UpdateButtonText);

            }
            catch (Exception ex)
            {
                await _userDialogs.AlertAsync(ex.Message);
            }
        }

        private void CharacteristicOnValueUpdated(object sender, CharacteristicUpdatedEventArgs characteristicUpdatedEventArgs)
        {
            byte[] datas = characteristicUpdatedEventArgs.Characteristic.Value;
            BleCommand cmd = (BleCommand)datas[0];
            //Messages.Insert(0, $"All CMD:  {cmd}");
            switch (cmd)
            {
                case BleCommand.BLE_CMD_STOVELOCK_GET:
                    Messages.Insert(0, $"StoveGet:  {cmd}");
                    break;
                case BleCommand.BLE_CMD_MAIN_INFO:
                    MainInfo = new BleMainInfo();
                    MainInfo.Parse(datas);
                    //Debug.WriteLine("StoveLock is:" + MainInfo.StoveLock);
                    //Debug.WriteLine("StoveLockPswd1 is:" + MainInfo.StoveLockPswd1);
                    //Debug.WriteLine("StoveLockPswd2 is:" + MainInfo.StoveLockPswd2);
                    //Debug.WriteLine("StoveLockPswd3 is:" + MainInfo.StoveLockPswd3);
                    //Debug.WriteLine("StoveLockPswd4 is:" + MainInfo.StoveLockPswd4);
                    //Debug.WriteLine("FwVersionMijor is:" + MainInfo.FwVersionMijor);
                    //Debug.WriteLine("FwVersionMinor is:" + MainInfo.FwVersionMinor);
                    //Debug.WriteLine("FwVersionPatch is:" + MainInfo.FwVersionPatch);
                    //Debug.WriteLine("SmokeLastSync is:" + MainInfo.SmokeLastSync);

                    Messages.Insert(0, $"StoveLock is:  {MainInfo.StoveLock}");
                    Messages.Insert(0, $"Password1: {MainInfo.StoveLockPswd1}"); //0
                    Messages.Insert(0, $"Password2: {MainInfo.StoveLockPswd2}"); //0
                    Messages.Insert(0, $"Password3: {MainInfo.StoveLockPswd3}"); //15
                    Messages.Insert(0, $"Password4: {MainInfo.StoveLockPswd4}"); //15
                    Messages.Insert(0, $"FwVersionMijor: {MainInfo.FwVersionMijor}");
                    Messages.Insert(0, $"FwVersionMinor {MainInfo.FwVersionMinor}");
                    Messages.Insert(0, $"FwVersionPatch: {MainInfo.FwVersionPatch}");
                    Messages.Insert(0, $"SmokeLastSync: {MainInfo.SmokeLastSync}");
                    break;
                case BleCommand.BLE_CMD_SENSOR_INFO:
                    bleSensorInfo = new BleSensorInfo();
                    bleSensorInfo.Parse(datas);
                    Debug.WriteLine("BatteryCapacity is:" + bleSensorInfo.BatteryCapacity);
                    Debug.WriteLine("SafetyProfile is:" + bleSensorInfo.SafetyProfile);
                    Debug.WriteLine("WorkingMode is:" + bleSensorInfo.WorkingMode);
                    Debug.WriteLine("BatteryVoltage is:" + bleSensorInfo.BatteryVoltage);
                    Debug.WriteLine("FirmwareVersionMajor is:" + bleSensorInfo.FirmwareVersionMajor);
                    Debug.WriteLine("FirmwareVersionMinor is:" + bleSensorInfo.FirmwareVersionMinor);
                    Debug.WriteLine("FirmwareVersionPatch is:" + bleSensorInfo.FirmwareVersionPatch);

                    Messages.Insert(0, $"BatteryCapacity:  {bleSensorInfo.BatteryCapacity}");
                    Messages.Insert(0, $"SafetyProfile: {bleSensorInfo.SafetyProfile}");
                    Messages.Insert(0, $"WorkingMode: {bleSensorInfo.WorkingMode}");
                    Messages.Insert(0, $"BatteryVoltage: {bleSensorInfo.BatteryVoltage}");
                    Messages.Insert(0, $"FirmwareVersionMajor: {bleSensorInfo.FirmwareVersionMajor}.{bleSensorInfo.FirmwareVersionMinor}.{bleSensorInfo.FirmwareVersionPatch}");
                    break;
                case BleCommand.BLE_CMD_COM_SYNC:
                    //Messages.Insert(0, $"Com_Sync:  {cmd}");
                    break;
                case BleCommand.BLE_CMD_TEMP_00_07:
                case BleCommand.BLE_CMD_TEMP_08_15:
                case BleCommand.BLE_CMD_TEMP_16_23:
                case BleCommand.BLE_CMD_TEMP_24_31:
                case BleCommand.BLE_CMD_TEMP_32_39:
                case BleCommand.BLE_CMD_TEMP_40_47:
                case BleCommand.BLE_CMD_TEMP_48_55:
                case BleCommand.BLE_CMD_TEMP_56_63:
                    {
                        //Messages.Insert(0, $"Start getting:  {cmd}");
                        if (cmd == BleCommand.BLE_CMD_TEMP_00_07)
                        {
                            intensities = new double[8, 8];
                        }

                        BleTempData tempData_ = new BleTempData();
                        tempData_.Parse(datas);


                        int row = (int)(cmd - BleCommand.BLE_CMD_TEMP_00_07);
                        for (int col = 0; col < 8; col++)
                        {
                            var val = tempData_.Values[col];
                            double value = tempData_.Values[col];
                            intensities[row, col] = value;
                        }
                        //    //logTemp.Update(row, col, val);
                        //    //simFilter.Update(row, col, val);
                        //}

                        //if (cmd == BleCommand.BLE_CMD_TEMP_56_63)
                        //{
                        //    double[] tempVal = new double[intensities.Length];
                        //    count = 0;
                        //    new Thread(() => Task.Run(() =>
                        //      {
                        //          foreach (double temputure in intensities)
                        //          {
                        //              tempVal[count] = temputure;
                        //          }
                        //      })).Start();

                        double[] tempVal = new double[intensities.Length];
                        foreach (double temputure in intensities)
                        {
                            tempVal[count] = temputure;
                            Messages.Insert(0, temputure.ToString());
                            count++;
                        }


                        //heatMap.Render();
                        //DrawThermalCameraSel(matrixSelect.GetRow(), matrixSelect.GetCol(), heatMap.GetBitmap());

                    }
                    break;
                case BleCommand.BLE_CMD_INVALID:
                    Messages.Insert(0, $"Cmd : {cmd}");
                    break;
                case BleCommand.BLE_CMD_SUCCESS:
                    Messages.Insert(0, $"Cmd : {cmd}");
                    break;
                case BleCommand.BLE_CMD_FAILURE:
                    Messages.Insert(0, $"Cmd : {cmd}");
                    break;
                case BleCommand.BLE_CMD_APP_STATE_SET:
                    BleSetup bleSetup = new BleSetup();
                    bleSetup.Parse(datas);
                    Messages.Insert(0, $"App State Enable:  {bleSetup.Enable}");
                    break;
                case BleCommand.BLE_CMD_MOVEMENT_STATE:
                    {
                        BleMoveData moveData = new BleMoveData();
                        moveData.Parse(datas);
                        Debug.WriteLine("Move:" + moveData.Movement);
                    }
                    break;
            }
        }

        //private async void GeneralSetup()
        //{
        //    byte[] BLE_CMD_APP_STATE_SET = null;
        //    bs.Enable = true;
        //    BLE_CMD_APP_STATE_SET = core.BleCommunication.Send(BleCommand.BLE_CMD_APP_STATE_SET, bs.GetBytes());
        //    await Characteristic.WriteAsync(BLE_CMD_APP_STATE_SET);
        //}2


        private void TempRender(object sender, EventArgs e)
        {







            //pltTemperature.plt.AxisAutoX();
            //pltTemperature.Render();

        }

        public MvxCommand GoToNextPage => new MvxCommand(GoToNextPageFunction);
        public async void GoToNextPageFunction()
        {
            //StopUpdate();
            byte[] BLE_CMD_APP_STATE_SET = null;
            BleSetup bs = new BleSetup();
            bs.Enable = false;
            BLE_CMD_APP_STATE_SET = core.BleCommunication.Send(BleCommand.BLE_CMD_APP_STATE_SET, bs.GetBytes());
            await Characteristic.WriteAsync(BLE_CMD_APP_STATE_SET);


            var navigation = Mvx.IoCProvider.Resolve<IMvxNavigationService>();
            Debug.WriteLine("Ok");
            if (Characteristic != null)
                await navigation.Navigate<LoadMoveModel, MvxBundle>(new MvxBundle(new Dictionary<string, string> { { string.Empty, string.Empty } }));
            await navigation.Close(this);
        }




    }
}
//    public class LoadTempModel : BaseViewModel
//    {
//        // General
//        private readonly IMvxNavigationService _navigation;
//        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();
//        public ICharacteristic Characteristic { get; private set; }

//        public IDescriptor Descriptor { get; set; }
//        public string CharacteristicValue => Characteristic?.Value.ToHexString().Replace("-", " ");

//        private readonly IUserDialogs _userDialogs;
//        public Core core = new Core();
//        public bool _updatesStarted;

//        public bool TempScanLock = true;
//        public string TempStatusText => TempScanLock ? "Temp Scan" : "Temp Lock";

//        BleMainInfo MainInfo = new BleMainInfo();

//        // Page Device Info
//        public BleSensorInfo bleSensorInfo = new BleSensorInfo();
//        public BleStoveLock bleStoveLock = new BleStoveLock();

//        // Communication Pair
//        //
//        BleTempScan bts = new BleTempScan();


//        //
//        BleMoveScan move = new BleMoveScan();
//        BleMoveData moveData = new BleMoveData();

//        //
//        Temp logTemp;
//        Filter simFilter;
//        MatrixSelect matrixSelect;
//        Timer tempRenderTimer;
//        Heatmap plotSignal = new Heatmap();
//        Colormap colormap = Colormap.Thermal;// = new Colorbar();
//        int chartRenderMax = 200;
//        double[,] Matrix;
//        DateTime timeStart;
//        public string Permissions            
//        {
//            get
//            {
//                if (Characteristic == null)
//                    return string.Empty;

//                return (Characteristic.CanRead ? "Read " : "") +
//                       (Characteristic.CanWrite ? "Write " : "") +
//                       (Characteristic.CanUpdate ? "Update" : "");
//            }
//        }

//        public LoadTempModel(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
//        {
//            _userDialogs = userDialogs;
//        }

//        public override async void Prepare(MvxBundle parameters)
//        {
//            base.Prepare(parameters);
//            foreach (var connectedDevice in Adapter.ConnectedDevices)
//            {
//                //    //update rssi for already connected evices (so tha 0 is not shown in the list)
//                //    try
//                //    {
//                //        Trace.Message("Prepare");
//                //        await connectedDevice.UpdateRssiAsync();
//                //        if (connectedDevice != null)
//                //        {
//                var Service = await connectedDevice.GetServiceAsync(Guid.Parse("69ddd530-8216-480e-a48d-a516ae310fc2"));

//                if (Service != null)
//                {
//                    Characteristic = await Service.GetCharacteristicAsync(Guid.Parse("d0962ce0-360e-4db6-a356-b443a20d94e3"));
//                }
//                //        }
//                //    }
//                //    catch (Exception ex)
//                //    {
//                //        await _userDialogs.AlertAsync($"Failed to update RSSI for {connectedDevice.Name}");
//                //    }
//            }



//            //LoadCommunicationPair();
//            //CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
//            //CancellationToken cancellationToken = cancellationTokenSource.Token;

//            //await Task.Delay(1000).ContinueWith(async (t) =>
//            //{
//            //    //LoadDeviceInfo();
//            //StartUpdate();
//            //}, cancellationToken);

//            //await new Task(async (t) =>
//            //{
//            //    await LoadTemp();
//            //}, cancellationToken);
//            StartUpdate();
//            //LoadTemp();
//            //LoadMovement();
//        }

//        public override void ViewAppeared()
//        {
//            base.ViewAppeared();

//            if (Characteristic != null)
//            {
//                return;
//            }

//            var navigation = Mvx.IoCProvider.Resolve<IMvxNavigationService>();
//            navigation.Close(this);
//        }

//        public override void ViewDisappeared()
//        {
//            base.ViewDisappeared();

//            if (Characteristic != null)
//            {
//                StopUpdate();
//            }

//        }

//       // public MvxCommand ReadCommand => new MvxCommand(ReadValueAsync);

//        //private async void ReadValueAsync()
//        //{
//        //    if (Characteristic == null)
//        //        return;

//        //    try
//        //    {
//        //        _userDialogs.ShowLoading("Reading characteristic value...");

//        //        await Characteristic.ReadAsync();

//        //        await RaisePropertyChanged(() => CharacteristicValue);

//        //        Messages.Insert(0, $"Read value {CharacteristicValue}");
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        _userDialogs.HideLoading();
//        //        await _userDialogs.AlertAsync(ex.Message);

//        //        Messages.Insert(0, $"Error {ex.Message}");

//        //    }
//        //    finally
//        //    {
//        //        _userDialogs.HideLoading();
//        //    }

//        //}

//        public  MvxCommand ToggleTempScan => new MvxCommand(() =>
//        {
//            if (TempScanLock)
//            {

//                TempScanLock = false;

//                StartScan();

//                RaisePropertyChanged (() => TempStatusText);
//            }
//            else
//            {
//                TempScanLock = true;

//                StopScan();

//                RaisePropertyChanged (() => TempStatusText);
//            }         
//        });

//        // PAGE TEMPRATURE RENDER 
//        public async Task LoadTemp()
//        {
//            StartUpdate();
//            //CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
//            //CancellationToken cancellationToken = cancellationTokenSource.Token;

//            await WriteTempSync();


//        }
//        private async void StartScan()
//        {
//            byte[] BLE_CMD_APP_STATE_SET = null;
//            BleSetup bs = new BleSetup();
//            bs.Enable = true;
//            BLE_CMD_APP_STATE_SET = core.BleCommunication.Send(BleCommand.BLE_CMD_APP_STATE_SET, bs.GetBytes());
//            await Characteristic.WriteAsync(BLE_CMD_APP_STATE_SET);
//            Messages.Insert(0, $"Set app state value true");

//            byte[] BLE_CMD_TEMP_SCAN = null;
//            bts.Enable = true;
//            BLE_CMD_TEMP_SCAN = core.BleCommunication.Send(BleCommand.BLE_CMD_TEMP_SCAN, bts.GetBytes());
//            await Characteristic.WriteAsync(BLE_CMD_TEMP_SCAN);
//            await RaisePropertyChanged(() => CharacteristicValue);
//            Messages.Insert(0, $"Wrote value Start");
//        }

//        private async void StopScan()
//        {
//            byte[] BLE_CMD_TEMP_SCAN = null;
//            bts.Enable = false;
//            BLE_CMD_TEMP_SCAN = core.BleCommunication.Send(BleCommand.BLE_CMD_TEMP_SCAN, bts.GetBytes());
//            await Characteristic.WriteAsync(BLE_CMD_TEMP_SCAN);
//            await RaisePropertyChanged(() => CharacteristicValue);
//            Messages.Insert(0, $"Wrote value Stop");
//        }

//        private async Task WriteTempSync()
//        {
//            byte[] BLE_CMD_APP_STATE_SET = null;
//            BleSetup bs = new BleSetup();
//            bs.Enable = true;
//            BLE_CMD_APP_STATE_SET = core.BleCommunication.Send(BleCommand.BLE_CMD_APP_STATE_SET, bs.GetBytes());
//            await Characteristic.WriteAsync(BLE_CMD_APP_STATE_SET);

//            //byte[] BLE_CMD_TEMP_SCAN = null;
//            //bts.Enable = true;
//            //BLE_CMD_TEMP_SCAN = core.BleCommunication.Send(BleCommand.BLE_CMD_TEMP_SCAN, bts.GetBytes());
//            //await Characteristic.WriteAsync(BLE_CMD_TEMP_SCAN);

//            //await RaisePropertyChanged(() => CharacteristicValue);
//            //Messages.Insert(0, $"Wrote value {CharacteristicValue}");

//            //Core.Core.BleCommunication.Send(Data.BleCommand.BLE_CMD_COM_SYNC, bsw.GetBytes());
//        }



//        // MOVE SCAN 2
//        public void LoadMovement()
//        {
//            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
//            CancellationToken cancellationToken = cancellationTokenSource.Token;

//            Task.Delay(2000).ContinueWith(async (t) =>
//            {
//                WriteMoveSync();
//            }, cancellationToken);

//        }
//        private async void WriteMoveSync()
//        {

//            byte[] BLE_CMD_APP_STATE_SET = null;
//            BleSetup bs = new BleSetup();
//            bs.Enable = true;
//            BLE_CMD_APP_STATE_SET = core.BleCommunication.Send(BleCommand.BLE_CMD_APP_STATE_SET, bs.GetBytes());
//            await Characteristic.WriteAsync(BLE_CMD_APP_STATE_SET);

//            //byte[] BLE_CMD_MOVE_SCAN = null;
//            //move.Enable = true;
//            //BLE_CMD_MOVE_SCAN = core.BleCommunication.Send(BleCommand.BLE_CMD_MOVE_SCAN, move.GetBytes());
//            //await Characteristic.WriteAsync(BLE_CMD_MOVE_SCAN);


//            await RaisePropertyChanged(() => CharacteristicValue);
//            Messages.Insert(0, $"Wrote value {CharacteristicValue}");
//        }

//        private async void StartUpdate()
//        {
//            try
//            {
//                Characteristic.ValueUpdated -= CharacteristicOnValueUpdated;
//                Characteristic.ValueUpdated += CharacteristicOnValueUpdated;

//                await Characteristic.StartUpdatesAsync();
//                Messages.Insert(0, $"Start updates");
//                //await RaisePropertyChanged(() => { StoveLock ? });
//            }
//            catch (Exception ex)
//            {
//                await _userDialogs.AlertAsync(ex.Message);
//            }

//        }

//        private async void StopUpdate()
//        {
//            try
//            {
//                _updatesStarted = false;

//                await Characteristic.StopUpdatesAsync();
//                Characteristic.ValueUpdated -= CharacteristicOnValueUpdated;
//                Messages.Insert(0, $"Stop updates");

//                //  await RaisePropertyChanged(() => UpdateButtonText);

//            }
//            catch (Exception ex)
//            {
//                await _userDialogs.AlertAsync(ex.Message);
//            }
//        }

//        private void CharacteristicOnValueUpdated(object sender, CharacteristicUpdatedEventArgs characteristicUpdatedEventArgs)
//        {
//            byte[] datas = characteristicUpdatedEventArgs.Characteristic.Value;
//            BleCommand cmd = (BleCommand)datas[0];
//            //Messages.Insert(0, $"All CMD:  {cmd}");
//            switch (cmd)
//            {
//                case BleCommand.BLE_CMD_STOVELOCK_GET:
//                    Messages.Insert(0, $"StoveGet:  {cmd}");
//                    break;
//                case BleCommand.BLE_CMD_MAIN_INFO:
//                    MainInfo = new BleMainInfo();
//                    MainInfo.Parse(datas);
//                    //Debug.WriteLine("StoveLock is:" + MainInfo.StoveLock);
//                    //Debug.WriteLine("StoveLockPswd1 is:" + MainInfo.StoveLockPswd1);
//                    //Debug.WriteLine("StoveLockPswd2 is:" + MainInfo.StoveLockPswd2);
//                    //Debug.WriteLine("StoveLockPswd3 is:" + MainInfo.StoveLockPswd3);
//                    //Debug.WriteLine("StoveLockPswd4 is:" + MainInfo.StoveLockPswd4);
//                    //Debug.WriteLine("FwVersionMijor is:" + MainInfo.FwVersionMijor);
//                    //Debug.WriteLine("FwVersionMinor is:" + MainInfo.FwVersionMinor);
//                    //Debug.WriteLine("FwVersionPatch is:" + MainInfo.FwVersionPatch);
//                    //Debug.WriteLine("SmokeLastSync is:" + MainInfo.SmokeLastSync);

//                    Messages.Insert(0, $"StoveLock is:  {MainInfo.StoveLock}");
//                    Messages.Insert(0, $"Password1: {MainInfo.StoveLockPswd1}"); //0
//                    Messages.Insert(0, $"Password2: {MainInfo.StoveLockPswd2}"); //0
//                    Messages.Insert(0, $"Password3: {MainInfo.StoveLockPswd3}"); //15
//                    Messages.Insert(0, $"Password4: {MainInfo.StoveLockPswd4}"); //15
//                    Messages.Insert(0, $"FwVersionMijor: {MainInfo.FwVersionMijor}");
//                    Messages.Insert(0, $"FwVersionMinor {MainInfo.FwVersionMinor}");
//                    Messages.Insert(0, $"FwVersionPatch: {MainInfo.FwVersionPatch}");
//                    Messages.Insert(0, $"SmokeLastSync: {MainInfo.SmokeLastSync}");
//                    break;
//                case BleCommand.BLE_CMD_SENSOR_INFO:
//                    bleSensorInfo = new BleSensorInfo();
//                    bleSensorInfo.Parse(datas);
//                    Debug.WriteLine("BatteryCapacity is:" + bleSensorInfo.BatteryCapacity);
//                    Debug.WriteLine("SafetyProfile is:" + bleSensorInfo.SafetyProfile);
//                    Debug.WriteLine("WorkingMode is:" + bleSensorInfo.WorkingMode);
//                    Debug.WriteLine("BatteryVoltage is:" + bleSensorInfo.BatteryVoltage);
//                    Debug.WriteLine("FirmwareVersionMajor is:" + bleSensorInfo.FirmwareVersionMajor);
//                    Debug.WriteLine("FirmwareVersionMinor is:" + bleSensorInfo.FirmwareVersionMinor);
//                    Debug.WriteLine("FirmwareVersionPatch is:" + bleSensorInfo.FirmwareVersionPatch);

//                    Messages.Insert(1, $"BatteryCapacity:  {bleSensorInfo.BatteryCapacity}");
//                    Messages.Insert(1, $"SafetyProfile: {bleSensorInfo.SafetyProfile}");
//                    Messages.Insert(1, $"WorkingMode: {bleSensorInfo.WorkingMode}");
//                    Messages.Insert(1, $"BatteryVoltage: {bleSensorInfo.BatteryVoltage}");
//                    Messages.Insert(1, $"FirmwareVer: {bleSensorInfo.FirmwareVersionMajor}.{bleSensorInfo.FirmwareVersionMinor}.{bleSensorInfo.FirmwareVersionPatch}");
//                    break;
//                case BleCommand.BLE_CMD_COM_SYNC:
//                    Messages.Insert(0, $"Com_Sync:  {cmd}");
//                    break;
//                case BleCommand.BLE_CMD_TEMP_00_07:
//                case BleCommand.BLE_CMD_TEMP_08_15:
//                case BleCommand.BLE_CMD_TEMP_16_23:
//                case BleCommand.BLE_CMD_TEMP_24_31:
//                case BleCommand.BLE_CMD_TEMP_32_39:
//                case BleCommand.BLE_CMD_TEMP_40_47:
//                case BleCommand.BLE_CMD_TEMP_48_55:
//                case BleCommand.BLE_CMD_TEMP_56_63:
//                    {
//                        Messages.Insert(0, $"Start getting:  {cmd}");
//                        if (cmd == BleCommand.BLE_CMD_TEMP_00_07)
//                        {
//                            int[] xs = Enumerable.Range(0, 200).ToArray();
//                            int[] ys = Enumerable.Range(0, 200).ToArray();
//                            double[,] intensities = new double[ys.Length, xs.Length];
//                            logTemp = new Temp();
//                        }

//                        BleTempData tempData_ = new BleTempData();
//                        tempData_.Parse(datas);
//                        Debug.WriteLine("Temp: " + datas.ToString());

//                        int row = (int)(cmd - BleCommand.BLE_CMD_TEMP_00_07);
//                        for (int col = 0; col < 8; col++)
//                        {                           
//                            var val = tempData_.Values[col];                            
//                            double value = tempData_.Values[col];

//                            logTemp.Update(row, col, val);
//                            simFilter.Update(row,col,val);

//                        }

//                        if (cmd == BleCommand.BLE_CMD_TEMP_56_63)
//                        {   
//                            for(int i = 0; i < 8; i++)
//                            {
//                                for (int j = 0; j < 8; j++)
//                                {
//                                    Debug.WriteLine(Matrix[i, j].ToString());
//                                }
//                            }
//                            plotSignal.Smooth = true;
//                            plotSignal.Update(Matrix, null, null, null);
//                            //heatMap.Render();
//                            //DrawThermalCameraSel(matrixSelect.GetRow(), matrixSelect.GetCol(), heatMap.GetBitmap());
//                        }
//                    }
//                    break;
//                case BleCommand.BLE_CMD_INVALID:
//                    Messages.Insert(0, $"Cmd : {cmd}");
//                    break;
//                case BleCommand.BLE_CMD_SUCCESS:
//                    Messages.Insert(0, $"Cmd : {cmd}");
//                    break;
//                case BleCommand.BLE_CMD_FAILURE:
//                    Messages.Insert(0, $"Cmd : {cmd}");
//                    break;
//                case BleCommand.BLE_CMD_APP_STATE_SET:
//                    BleSetup bleSetup = new BleSetup();
//                    bleSetup.Parse(datas);
//                    Messages.Insert(0, $"App State Enable:  {bleSetup.Enable}");
//                    break;
//                case BleCommand.BLE_CMD_MOVEMENT_STATE:
//                    {
//                        BleMoveData moveData = new BleMoveData();
//                        moveData.Parse(datas);
//                        Debug.WriteLine("Move:" + moveData.Movement);
//                    }
//                    break;
//            }
//        }

//        //private async void GeneralSetup()
//        //{
//        //    byte[] BLE_CMD_APP_STATE_SET = null;
//        //    bs.Enable = true;
//        //    BLE_CMD_APP_STATE_SET = core.BleCommunication.Send(BleCommand.BLE_CMD_APP_STATE_SET, bs.GetBytes());
//        //    await Characteristic.WriteAsync(BLE_CMD_APP_STATE_SET);
//        //}

//        private void TempRender(object sender, EventArgs e)
//        {
//            int row = matrixSelect.GetRow();
//            int col = matrixSelect.GetCol();
//            if (logTemp.Matrix[7 - row, col].GetLogMax() > 0)
//            {
//                plotSignal.ScaleMax = logTemp.Matrix[7 - row, col].GetLogMax() - 1;


//                    if (plotSignal.ScaleMax >= chartRenderMax)
//                    {
//                        plotSignal.ScaleMin = plotSignal.ScaleMax - chartRenderMax;
//                    }

//                    plotSignal.ScaleMin = 0;


//                //pltTemperature.plt.AxisAutoX();
//                //pltTemperature.Render();
//            }
//        }

//    }    
//}