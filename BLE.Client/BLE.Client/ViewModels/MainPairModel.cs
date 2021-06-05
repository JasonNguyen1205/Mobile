using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using BLE.Client.Function;
using BLE.Client.Object;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using Xamarin.Forms;
using Trace = Plugin.BLE.Abstractions.Trace;

namespace BLE.Client.ViewModels
{
    public class MainPairModel : BaseViewModel
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

     
    public string Selection { get; set; }
        public bool MainFlag { get; set; }
        
        public string MainFlagText => MainFlag ? "Pair With Main" : "Pair With Main + Smoke";

        BleMainInfo MainInfo = new BleMainInfo();

        // Page Device Info
        public BleSensorInfo bleSensorInfo = new BleSensorInfo();
        public BleStoveLock bleStoveLock = new BleStoveLock();

        // Communication Pair
        //
        BleTempScan bts = new BleTempScan();

        //
        BleMoveScan move = new BleMoveScan();
        BleMoveData moveData = new BleMoveData();

        public string Permissions
        {
            get
            {
                if (Characteristic == null)
                    return string.Empty;

                return (Characteristic.CanRead ? "Read " : "") +
                       (Characteristic.CanWrite ? "Write " : "") +
                       (Characteristic.CanUpdate ? "Update" : "");
            }
        }

        public MainPairModel(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;
            MainFlag = true;
        }

        public override async void Prepare(MvxBundle parameters)
        {
            base.Prepare(parameters);
            foreach (var connectedDevice in Adapter.ConnectedDevices)
            {
                //update rssi for already connected evices (so tha 0 is not shown in the list)
                try
                {
                    Trace.Message("Prepare");
                    //await connectedDevice.UpdateRssiAsync();
                    if (connectedDevice != null)
                    {
                        var Service = await connectedDevice.GetServiceAsync(Guid.Parse("69ddd530-8216-480e-a48d-a516ae310fc2"));

                        if (Service != null)
                        {
                            Characteristic = await Service.GetCharacteristicAsync(Guid.Parse("d0962ce0-360e-4db6-a356-b443a20d94e3"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    await _userDialogs.AlertAsync($"Failed to update RSSI for {connectedDevice.Name}");
                }
            }

            //StartUpdate();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            await Task.Delay(2000).ContinueWith(async (t) =>
            {
              LoadCommunicationPair();
            }, cancellationToken);

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

        public void LoadCommunicationPair()
        {
            StartUpdate();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Task.Delay(2000).ContinueWith(async (t) =>
            {
                SendToSetupCommunicationPair();
            }, cancellationToken);

        }

        private async void SendToSetupCommunicationPair()
        {
            byte[] BLE_CMD_APP_STATE_SET = null;
            BleSetup bs = new BleSetup();
            bs.Enable = true;
            BLE_CMD_APP_STATE_SET = core.BleCommunication.Send(BleCommand.BLE_CMD_APP_STATE_SET, bs.GetBytes());
            await Characteristic.WriteAsync(BLE_CMD_APP_STATE_SET);

            bs.Enable = false;
            BLE_CMD_APP_STATE_SET = core.BleCommunication.Send(BleCommand.BLE_CMD_APP_STATE_SET, bs.GetBytes());
            await Characteristic.WriteAsync(BLE_CMD_APP_STATE_SET);


            await RaisePropertyChanged(() => CharacteristicValue);
            Messages.Insert(0, $"Wrote value {CharacteristicValue}");
        }
        public MvxCommand OnChangePair => new MvxCommand(SetPairMain);
        public async void SetPairMain()
        {
            BleSyncWord bsw = new BleSyncWord();

            bsw.SType = (BleSyncWord.SyncType)(int.Parse(Selection));

            bsw.MainSyncWords[0] = Convert.ToByte("00", 16);
            bsw.MainSyncWords[1] = Convert.ToByte("0F", 16);
            bsw.MainSyncWords[2] = Convert.ToByte("42", 16);
            bsw.MainSyncWords[3] = Convert.ToByte("40", 16);

            if (bsw.SType == (BleSyncWord.SyncType)1)
            {

                bsw.SmokeSyncWords[0] = Convert.ToByte("00", 16);
                bsw.SmokeSyncWords[1] = Convert.ToByte("98", 16);
                bsw.SmokeSyncWords[2] = Convert.ToByte("96", 16);
                bsw.SmokeSyncWords[3] = Convert.ToByte("86", 16);
            }

            byte[] BLE_CMD_COM_SYNC = null;
            BLE_CMD_COM_SYNC = core.BleCommunication.Send(BleCommand.BLE_CMD_COM_SYNC, bsw.GetBytes());
            await Characteristic.WriteAsync(BLE_CMD_COM_SYNC);

            BleWorkingMode bwm = new BleWorkingMode();
            bwm.Value = (WorkingMode)1;
            byte[] BLE_CMD_MODE_STATE = core.BleCommunication.Send(BleCommand.BLE_CMD_MODE_STATE, bwm.GetBytes());
            await Characteristic.WriteAsync(BLE_CMD_MODE_STATE);
        }


        private async void StartUpdate()
        {
            try
            {
                Characteristic.ValueUpdated -= CharacteristicOnValueUpdated;
                Characteristic.ValueUpdated += CharacteristicOnValueUpdated;

                await Characteristic.StartUpdatesAsync();
                Messages.Insert(0, $"Start updates");
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
                Messages.Insert(0, $"Stop updates");

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
           // Messages.Insert(0, $"All CMD:  {cmd}");
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

                    Messages.Insert(1, $"BatteryCapacity:  {bleSensorInfo.BatteryCapacity}");
                    Messages.Insert(1, $"SafetyProfile: {bleSensorInfo.SafetyProfile}");
                    Messages.Insert(1, $"WorkingMode: {bleSensorInfo.WorkingMode}");
                    Messages.Insert(1, $"BatteryVoltage: {bleSensorInfo.BatteryVoltage}");
                    Messages.Insert(1, $"FirmwareVersionMajor: {bleSensorInfo.FirmwareVersionMajor}.{bleSensorInfo.FirmwareVersionMinor}.{bleSensorInfo.FirmwareVersionPatch}");
                    break;
                case BleCommand.BLE_CMD_COM_SYNC:
                    Messages.Insert(0, $"Com_Sync:  {cmd}");
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
                        BleTempData tempData = new BleTempData();
                        tempData.Parse(datas);
                        Debug.WriteLine("Temp: " + datas.ToHexString());

                        //int row = (int)(cmd - BleCommand.BLE_CMD_TEMP_00_07);
                        //for (int col = 0; col < 8; col++)
                        //{
                        //    logTemp.Update(row, col, tempData.Values[col]);
                        //    simFilter.Update(row, col, tempData.Values[col]);
                        //    heatMap.UpdateMatrix(row, col, tempData.Values[col]);
                        //    heatMap.TrendUpdate(row, col, simFilter.GetTrend(row, col));
                        //}

                        //if (cmd == Data.BleCommand.BLE_CMD_TEMP_56_63)
                        //{
                        //    heatMap.Render();
                        //    DrawThermalCameraSel(matrixSelect.GetRow(), matrixSelect.GetCol(), heatMap.GetBitmap());
                        //}
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
        //}



        public MvxCommand GoToNextPage => new MvxCommand(GoToNextPageFunction);
        public async void GoToNextPageFunction()
        {
            //StopUpdate();

            var navigation = Mvx.IoCProvider.Resolve<IMvxNavigationService>();
            Debug.WriteLine("Ok");
            if (Characteristic != null)
                await navigation.Navigate<LoadTempModel, MvxBundle>(new MvxBundle(new Dictionary<string, string> { { "", "" } }));
        }

    }    
}