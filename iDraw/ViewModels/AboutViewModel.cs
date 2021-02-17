using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace iDraw.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "iDraw";

            Button0 = "whole.png";
            Button1 = "greenOn.png";
            Button2 = "lightBlueOff.png";
            Button3 = "darkBlueOff.png";
            Button4 = "yellowOff.png";
            Button5 = "redOff.png";
            Button6 = "orangeOff.png";
            Button7 = "eraserOff.png";

            Divider = false;
            Divider2 = false;

            SliderValue = 0.2;

            Button1Command = new Command(() =>
            {
                switchOff();
                Button1 = "greenOn.png";
            });
            Button2Command = new Command(() =>
            {
                switchOff();
                Button2 = "lightBlueOn.png";
            });
            Button3Command = new Command(() =>
            {
                switchOff();
                Button3 = "darkBlueOn.png";
            });
            Button4Command = new Command(() =>
            {
                switchOff();
                Button4 = "yellowOn.png";
            });
            Button5Command = new Command(() =>
            {
                switchOff();
                Button5 = "redOn.png";
            });
            Button6Command = new Command(() =>
            {
                switchOff();
                Button6 = "orangeOn.png";
            });
            Button7Command = new Command(() =>
            {
                switchOff();
                Button7 = "eraserOn.png";
            });
            DividerCommand = new Command(() =>
            {
                if (Button0.Equals("whole.png"))
                {
                    Button0 = "divided.png";
                    Divider = true;
                }
                else if(Button0.Equals("divided.png"))
                {
                    Button0 = "quad.png";
                    Divider2 = true;
                }
                else
                {
                    Button0 = "whole.png";
                    Divider = false;
                    Divider2 = false;
                }
            });
        }

        public Command Button1Command { get; }
        public Command Button2Command { get; }
        public Command Button3Command { get; }
        public Command Button4Command { get; }
        public Command Button5Command { get; }
        public Command Button6Command { get; }
        public Command Button7Command { get; }
        public Command DividerCommand { get; }

        void switchOff()
        {
            string[] offList = { "greenOff.png", "lightBlueOff.png", "darkBlueOff.png", "yellowOff.png", "redOff.png", "orangeOff.png", "eraserOff.png" };
            Button1 = offList[0];
            Button2 = offList[1];
            Button3 = offList[2];
            Button4 = offList[3];
            Button5 = offList[4];
            Button6 = offList[5];
            Button7 = offList[6];
        }
    }
}