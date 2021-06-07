using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Mobile.Function;
using Mobile.Object;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;

namespace Mobile.ViewModels
{
    public class ConfigViewModel : BaseViewModel
    {
        // General
        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();
        public ICharacteristic Characteristic { get; private set; }
        public string CharacteristicValue => Characteristic?.Value.ToHexString().Replace("-", " ");
        private readonly IUserDialogs _userDialogs;
        public Core core = new Core();
        public bool _updatesStarted;

        BleMainInfo MainInfo = new BleMainInfo();
        // Page Device Info
        public BleSensorInfo bleSensorInfo = new BleSensorInfo();
        public BleStoveLock bleStoveLock = new BleStoveLock();
        public bool StoveLock = false;
        public string UpdateLockText => StoveLock ? "Lock" : "Unlock";


        // Communication Pair

        //
        BleTempScan bts = new BleTempScan();
        BleSetup bs = new BleSetup();

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

        public ConfigViewModel(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
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
                    await connectedDevice.UpdateRssiAsync();
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
            StartUpdate();


            //LoadCommunicationPair();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            await Task.Delay(1000).ContinueWith(async (t) =>
            {
                LoadDeviceInfo();
            }, cancellationToken);

            //LoadTemp();
            //LoadMovement();
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

        //public MvxCommand ReadCommand => new MvxCommand(ReadValueAsync);

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

        public MvxCommand WriteCommand => new MvxCommand(WriteValueAsync);
        public void LoadDeviceInfo()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Task.Delay(1000).ContinueWith(async (t) =>
            {
                WriteValueAsync();
            }, cancellationToken);

        }

        private async void WriteValueAsync()
        {
            try
            {
                byte[] BLE_CMD_SENSOR_INFO = null;
                BLE_CMD_SENSOR_INFO = core.BleCommunication.Send(BleCommand.BLE_CMD_SENSOR_INFO, bleSensorInfo.GetBytes());
                await Characteristic.WriteAsync(BLE_CMD_SENSOR_INFO);

                BleMainInfo BLE_CMD_MAIN_INFO_MODEL = new BleMainInfo();
                byte[] BLE_CMD_MAIN_INFO = null;
                BLE_CMD_MAIN_INFO = core.BleCommunication.Send(BleCommand.BLE_CMD_MAIN_INFO, BLE_CMD_MAIN_INFO_MODEL.GetBytes());
                await Characteristic.WriteAsync(BLE_CMD_MAIN_INFO);

                //byte[] BLE_CMD_APP_STATE_SET = null;
                //BleSetup BLE_CMD_APP_STATE_SET_MODEL = new BleSetup();
                //BLE_CMD_APP_STATE_SET_MODEL.Enable = true;
                //BLE_CMD_APP_STATE_SET = core.BleCommunication.Send(BleCommand.BLE_CMD_APP_STATE_SET, BLE_CMD_APP_STATE_SET_MODEL.GetBytes());
                //await Characteristic.WriteAsync(BLE_CMD_APP_STATE_SET);

                BleSyncWord bsw = new BleSyncWord();
                bsw.SType = (BleSyncWord.SyncType)(0);
                bsw.MainSyncWords[0] = Convert.ToByte("00", 16);
                bsw.MainSyncWords[1] = Convert.ToByte("0F", 16);
                bsw.MainSyncWords[2] = Convert.ToByte("42", 16);
                bsw.MainSyncWords[3] = Convert.ToByte("40", 16);

                //////if (BleSyncWord.SyncType.MAIN_SMOKE == (BleSyncWord.SyncType)1)
                //////{

                //////    bsw.SmokeSyncWords[0] = Convert.ToByte("00", 16);
                //////    bsw.SmokeSyncWords[1] = Convert.ToByte("98", 16);
                //////    bsw.SmokeSyncWords[2] = Convert.ToByte("96", 16);
                //////    bsw.SmokeSyncWords[3] = Convert.ToByte("86", 16);
                //////}

                //byte[] BLE_CMD_COM_SYNC = core.BleCommunication.Send(BleCommand.BLE_CMD_COM_SYNC, bsw.GetBytes());
                //await Characteristic.WriteAsync(BLE_CMD_COM_SYNC);


                //BleWorkingMode bwm = new BleWorkingMode();
                //bwm.Value = (WorkingMode)1;
                //byte[] BLE_CMD_MODE_STATE = core.BleCommunication.Send(BleCommand.BLE_CMD_MODE_STATE, bwm.GetBytes());
                //await Characteristic.WriteAsync(BLE_CMD_MODE_STATE);


                //BleSetup BLE_CMD_APP_STATE_SET_MODEL_OUT = new BleSetup();
                //BLE_CMD_APP_STATE_SET_MODEL_OUT.Enable = false;
                //byte[] BLE_CMD_APP_STATE_SET_OUT = core.BleCommunication.Send(BleCommand.BLE_CMD_APP_STATE_SET, BLE_CMD_APP_STATE_SET_MODEL_OUT.GetBytes());
                //await Characteristic.WriteAsync(BLE_CMD_APP_STATE_SET_OUT);

                await RaisePropertyChanged(() => CharacteristicValue);
                //Messages.Insert(0, $"Wrote value {CharacteristicValue}");
            }
            catch (Exception ex)
            {
                _userDialogs.HideLoading();
                await _userDialogs.AlertAsync(ex.Message);
            }
        }


        public MvxCommand ToggleStoveLock => new MvxCommand((() =>
        {
            if (StoveLock)
            {
                StoveLock = false;
                RaisePropertyChanged(() => UpdateLockText);
            }
            else
            {
                StoveLock = true;
                RaisePropertyChanged(() => UpdateLockText);
            }
            SetupStoveLock();

        }));

        public async void SetupStoveLock()
        {
            //BleMainInfo BLE_CMD_MAIN_INFO_MODEL = new BleMainInfo();
            //byte[] BLE_CMD_MAIN_INFO = null;
            //BLE_CMD_MAIN_INFO = core.BleCommunication.Send(BleCommand.BLE_CMD_MAIN_INFO, BLE_CMD_MAIN_INFO_MODEL.GetBytes());
            //await Characteristic.WriteAsync(BLE_CMD_MAIN_INFO);

            await Task.Run(async () => {

                BleStoveLock BLE_CMD_STOVELOCK_SET_MODEL = new BleStoveLock();
                if (MainInfo.StoveLock)
                {
                    MainInfo.StoveLock = false;
                    BLE_CMD_STOVELOCK_SET_MODEL.Lock = MainInfo.StoveLock;
                    BLE_CMD_STOVELOCK_SET_MODEL.Pswd1 = int.Parse("1");
                    BLE_CMD_STOVELOCK_SET_MODEL.Pswd2 = int.Parse("2");
                    BLE_CMD_STOVELOCK_SET_MODEL.Pswd3 = int.Parse("3");
                    BLE_CMD_STOVELOCK_SET_MODEL.Pswd4 = int.Parse("4");
                    //BLE_CMD_STOVELOCK_SET_MODEL.Pswd1 = int.Parse(MainInfo.StoveLockPswd1.ToString());
                    //BLE_CMD_STOVELOCK_SET_MODEL.Pswd2 = int.Parse(MainInfo.StoveLockPswd2.ToString());
                    //BLE_CMD_STOVELOCK_SET_MODEL.Pswd3 = int.Parse(MainInfo.StoveLockPswd3.ToString());
                    //BLE_CMD_STOVELOCK_SET_MODEL.Pswd4 = int.Parse(MainInfo.StoveLockPswd4.ToString());
                }
                else
                {
                    MainInfo.StoveLock = true;
                    BLE_CMD_STOVELOCK_SET_MODEL.Lock = MainInfo.StoveLock;
                    BLE_CMD_STOVELOCK_SET_MODEL.Pswd1 = int.Parse("1");
                    BLE_CMD_STOVELOCK_SET_MODEL.Pswd2 = int.Parse("2");
                    BLE_CMD_STOVELOCK_SET_MODEL.Pswd3 = int.Parse("3");
                    BLE_CMD_STOVELOCK_SET_MODEL.Pswd4 = int.Parse("4");
                }

                byte[] BLE_CMD_STOVELOCK_SET = core.BleCommunication.Send(BleCommand.BLE_CMD_STOVELOCK_SET, BLE_CMD_STOVELOCK_SET_MODEL.GetBytes());
                await Characteristic.WriteAsync(BLE_CMD_STOVELOCK_SET);
            });



            //BLE_CMD_STOVELOCK_SET_MODEL.Pswd1 = MainInfo.StoveLockPswd1;
            //BLE_CMD_STOVELOCK_SET_MODEL.Pswd2 = MainInfo.StoveLockPswd2;
            //BLE_CMD_STOVELOCK_SET_MODEL.Pswd3 = MainInfo.StoveLockPswd3;
            //BLE_CMD_STOVELOCK_SET_MODEL.Pswd4 = MainInfo.StoveLockPswd4;

            //BLE_CMD_STOVELOCK_SET_MODEL.Pswd1 = int.Parse("00");
            //BLE_CMD_STOVELOCK_SET_MODEL.Pswd2 = int.Parse("00");
            //BLE_CMD_STOVELOCK_SET_MODEL.Pswd3 = int.Parse("15");
            //BLE_CMD_STOVELOCK_SET_MODEL.Pswd4 = int.Parse("15");
        }

        public void CreateModel()
        {


        }

        // Page Communication Pair---------------------------------------------------------------------------------------------------------------
        public void LoadCommunicationPair()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Task.Delay(1000).ContinueWith(async (t) =>
            {
                SendToSetupCommunicationPair();
            }, cancellationToken);

        }
        private async void SendToSetupCommunicationPair()
        {
            byte[] BLE_CMD_APP_STATE_SET = null;
            bs.Enable = true;
            BLE_CMD_APP_STATE_SET = core.BleCommunication.Send(BleCommand.BLE_CMD_APP_STATE_SET, bs.GetBytes());
            await Characteristic.WriteAsync(BLE_CMD_APP_STATE_SET);

            BleSyncWord bsw = new BleSyncWord();
            byte sw1, sw2, sw3, sw4;

            bsw.SType = (BleSyncWord.SyncType)(0);

            bsw.MainSyncWords[0] = Convert.ToByte("00", 16);
            bsw.MainSyncWords[1] = Convert.ToByte("0F", 16);
            bsw.MainSyncWords[2] = Convert.ToByte("42", 16);
            bsw.MainSyncWords[3] = Convert.ToByte("40", 16);

            //if (BleSyncWord.SyncType.MAIN_SMOKE == (BleSyncWord.SyncType)1)
            //{

            //    bsw.SmokeSyncWords[0] = Convert.ToByte("00", 16);
            //    bsw.SmokeSyncWords[1] = Convert.ToByte("98", 16);
            //    bsw.SmokeSyncWords[2] = Convert.ToByte("96", 16);
            //    bsw.SmokeSyncWords[3] = Convert.ToByte("86", 16);
            //}

            byte[] BLE_CMD_COM_SYNC = null;
            BLE_CMD_COM_SYNC = core.BleCommunication.Send(BleCommand.BLE_CMD_COM_SYNC, bsw.GetBytes());
            await Characteristic.WriteAsync(BLE_CMD_COM_SYNC);

            BleWorkingMode bwm = new BleWorkingMode();
            bwm.Value = (WorkingMode)1;
            byte[] BLE_CMD_MODE_STATE = core.BleCommunication.Send(BleCommand.BLE_CMD_MODE_STATE, bwm.GetBytes());
            await Characteristic.WriteAsync(BLE_CMD_MODE_STATE);


            BleSetup BLE_CMD_APP_STATE_SET_MODEL_OUT = new BleSetup();
            BLE_CMD_APP_STATE_SET_MODEL_OUT.Enable = false;
            byte[] BLE_CMD_APP_STATE_SET_OUT = core.BleCommunication.Send(BleCommand.BLE_CMD_APP_STATE_SET, BLE_CMD_APP_STATE_SET_MODEL_OUT.GetBytes());
            await Characteristic.WriteAsync(BLE_CMD_APP_STATE_SET_OUT);

            await RaisePropertyChanged(() => CharacteristicValue);
            //Messages.Insert(0, $"Wrote value {CharacteristicValue}");
        }

        // PAGE TEMPRATURE RENDER 
        public void LoadTemp()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Task.Delay(2000).ContinueWith(async (t) =>
            {
                WriteTempSync();
            }, cancellationToken);

        }
        private async void WriteTempSync()
        {
            byte[] BLE_CMD_APP_STATE_SET = null;
            bs.Enable = true;
            BLE_CMD_APP_STATE_SET = core.BleCommunication.Send(BleCommand.BLE_CMD_APP_STATE_SET, bs.GetBytes());
            await Characteristic.WriteAsync(BLE_CMD_APP_STATE_SET);

            byte[] BLE_CMD_TEMP_SCAN = null;
            bts.Enable = true;
            BLE_CMD_TEMP_SCAN = core.BleCommunication.Send(BleCommand.BLE_CMD_TEMP_SCAN, bts.GetBytes());
            await Characteristic.WriteAsync(BLE_CMD_TEMP_SCAN);

            await RaisePropertyChanged(() => CharacteristicValue);
           // Messages.Insert(0, $"Wrote value {CharacteristicValue}");

            //Core.Core.BleCommunication.Send(Data.BleCommand.BLE_CMD_COM_SYNC, bsw.GetBytes());
        }



        // MOVE SCAN 2
        public void LoadMovement()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Task.Delay(2000).ContinueWith(async (t) =>
            {
                WriteMoveSync();
            }, cancellationToken);

        }
        private async void WriteMoveSync()
        {

            byte[] BLE_CMD_APP_STATE_SET = null;
            bs.Enable = true;
            BLE_CMD_APP_STATE_SET = core.BleCommunication.Send(BleCommand.BLE_CMD_APP_STATE_SET, bs.GetBytes());
            await Characteristic.WriteAsync(BLE_CMD_APP_STATE_SET);

            byte[] BLE_CMD_MOVE_SCAN = null;
            move.Enable = true;
            BLE_CMD_MOVE_SCAN = core.BleCommunication.Send(BleCommand.BLE_CMD_MOVE_SCAN, move.GetBytes());
            await Characteristic.WriteAsync(BLE_CMD_MOVE_SCAN);


            await RaisePropertyChanged(() => CharacteristicValue);
            Messages.Insert(0, $"Wrote value {CharacteristicValue}");
        }

        private async void StartUpdate()
        {
            try
            {
                Characteristic.ValueUpdated -= CharacteristicOnValueUpdated;
                Characteristic.ValueUpdated += CharacteristicOnValueUpdated;

                await Characteristic.StartUpdatesAsync();
                //Messages.Insert(0, $"Start updates");
                //  await RaisePropertyChanged(() => UpdateButtonText);
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
                    //Messages.Insert(0, $"StoveGet:  {cmd}");
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
                    Messages.Insert(0, $"FirmwareVersion: {bleSensorInfo.FirmwareVersionMajor}:{bleSensorInfo.FirmwareVersionMinor}:{bleSensorInfo.FirmwareVersionPatch}");
                    //Messages.Insert(0, $"FirmwareVersionMinor: {bleSensorInfo.FirmwareVersionMinor}");
                    //Messages.Insert(0, $"FirmwareVersionPatch: {bleSensorInfo.FirmwareVersionPatch}");
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
            //byte[] BLE_CMD_APP_STATE_SET = null;
            //BleSetup bs = new BleSetup();
            //bs.Enable = true;
            //BLE_CMD_APP_STATE_SET = core.BleCommunication.Send(BleCommand.BLE_CMD_APP_STATE_SET, bs.GetBytes());
            //await Characteristic.WriteAsync(BLE_CMD_APP_STATE_SET);


            var navigation = Mvx.IoCProvider.Resolve<IMvxNavigationService>();
            Debug.WriteLine("Ok");
            if (Characteristic != null)
                await navigation.Navigate<MainPairModel, MvxBundle>(new MvxBundle(new Dictionary<string, string> { { "", "" } }));
        }

        public MvxCommand GoToTempPage => new MvxCommand(GoToTempPageFunction);
        public async void GoToTempPageFunction()
        {
            //StopUpdate();
            //byte[] BLE_CMD_APP_STATE_SET = null;
            //BleSetup bs = new BleSetup();
            //bs.Enable = true;
            //BLE_CMD_APP_STATE_SET = core.BleCommunication.Send(BleCommand.BLE_CMD_APP_STATE_SET, bs.GetBytes());
            //await Characteristic.WriteAsync(BLE_CMD_APP_STATE_SET);


            var navigation = Mvx.IoCProvider.Resolve<IMvxNavigationService>();
            Debug.WriteLine("Ok");
            if (Characteristic != null)
                await navigation.Navigate<LoadTempModel, MvxBundle>(new MvxBundle(new Dictionary<string, string> { { CharacteristicIdKey, Characteristic.Id.ToString() } }));
            await navigation.Close(this);
        }
    }
}