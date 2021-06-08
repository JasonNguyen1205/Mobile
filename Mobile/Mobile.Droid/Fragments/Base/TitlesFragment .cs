using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;

namespace Mobile.Droid.Fragments.Base
{
    public abstract class TitlesFragment : Fragment
    {

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(HeatmapId, null);
        }

        public abstract int HeatmapId { get; set; }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            InitExample();
        }

        protected abstract void InitExample();

        public virtual void InitExampleForUiTest()
        {
        }

    }
}