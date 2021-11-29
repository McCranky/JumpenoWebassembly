using JumpenoWebassembly.Client.Services;
using JumpenoWebassembly.Shared.Jumpeno.Entities;
using JumpenoWebassembly.Shared.Models;
using JumpenoWebassembly.Shared.Utilities;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using static JumpenoWebassembly.Shared.Jumpeno.Enums;

namespace JumpenoWebassembly.Client.Pages
{
    public partial class SkinEdit
    {
        [Inject] public IAuthService Auth { get; set; }
        [Inject] public UserService UserService { get; set; }

        public Dictionary<string, Animation> Skins { get; set; }
        public string SkinName { get; set; } = "";
        private int imageFrame = 0;
        private Timer _timer;
        private User _user;

        protected override async Task OnInitializedAsync()
        {
            _user = await Auth.GetUser();
            SkinName = _user.Skin;

            Skins = new Dictionary<string, Animation>(5);
            var bodySize = new Vector(0, 0);
            Skins.Add("mageSprite_fire", new Animation("mageSprite_fire.png", new Vector(4, 3), out _));
            Skins.Add("mageSprite_aer", new Animation("mageSprite_aer.png", new Vector(4, 3), out _));
            Skins.Add("mageSprite_earth", new Animation("mageSprite_earth.png", new Vector(4, 3), out _));
            Skins.Add("mageSprite_water", new Animation("mageSprite_water.png", new Vector(4, 3), out _));
            Skins.Add("mageSprite_magic", new Animation("mageSprite_magic.png", new Vector(4, 3), out _));

            _timer = new Timer(10000.0 / 60);
            _timer.Elapsed += async (sender, e) => await Tick(sender, e);
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private async Task Tick(Object source, ElapsedEventArgs e)
        {
            ++imageFrame;
            await InvokeAsync(StateHasChanged);
        }

        private async Task ChooseSkin()
        {
            _user.Skin = SkinName;
            await UserService.UpdateSkin(_user);
            StateHasChanged();
        }

        private void OnSkinSelection(string name)
        {
            SkinName = name;
        }

        private string GetCurrentFrameStyle(AnimationState state)
        {
            if (String.IsNullOrEmpty(SkinName)) {
                return "";
            }
            return Skins[SkinName].GetFrameStyle(state, imageFrame);
        }

        private string GetFirstFrameStyle(AnimationState state)
        {
            if (String.IsNullOrEmpty(SkinName)) {
                return "";
            }
            if (state == AnimationState.Falling) {
                return Skins[SkinName].GetFrameStyle(state, 1);
            } else if (state == AnimationState.Dead) {
                return Skins[SkinName].GetFrameStyle(AnimationState.Falling, 0);
            }
            return Skins[SkinName].GetFrameStyle(state, 0);
        }
    }
}
