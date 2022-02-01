using Blazored.Toast.Services;
using JumpenoWebassembly.Shared.Jumpeno.Utilities;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Client.Pages
{
    public partial class MapEdit
    {
        [Inject] public HttpClient Http { get; set; }
        [Inject] public IToastService Toast { get; set; }
        private Dictionary<int, MapTemplate> _maps;
        private MapTemplate _mapTemplate;
        private int _mapId;
        private int _editorMode;
        private readonly int _tileSize = JumpenoWebassembly.Shared.Constants.MapC.TileSize;

        protected override async Task OnParametersSetAsync()
        {
            var maps = await Http.GetFromJsonAsync<List<MapTemplate>>("api/game/maps");
            _maps = new Dictionary<int, MapTemplate>();
            foreach (var map in maps) {
                _maps.Add(map.Id, map);
            }
        }


        private void HandleEditorChanged(ChangeEventArgs args)
        {
            _mapTemplate = null;
            _editorMode = int.Parse(args.Value.ToString() ?? "-1");
            if (_editorMode == 2) {
                CreateMapTemplate();
            }
        }

        private async Task RemoveMap()
        {
            if (_maps.ContainsKey(_mapId)) {
                _maps.Remove(_mapId);
                await Http.GetAsync($"api/game/delmap/{_mapId}");
                Toast.ShowSuccess("Map removed.");
                _editorMode = 2;
                CreateMapTemplate();
            } else {
                CreateMapTemplate();
            }
        }

        private void HandleMapSelect(ChangeEventArgs args)
        {
            _mapId = int.Parse(args.Value.ToString() ?? "-1");
            if (_maps.TryGetValue(_mapId, out _mapTemplate)) {
                Toast.ShowInfo($"Map {_mapTemplate.Name} selected.");
            }
        }

        private void ChangeTile(int x, int y)
        {
            int index = _mapTemplate.Width * y + x;

            var tileStatus = _mapTemplate.Tiles[index] == '1' ? "0" : "1";
            _mapTemplate.Tiles = _mapTemplate.Tiles.Remove(index, 1).Insert(index, tileStatus);

            Console.WriteLine($@"[X:{x}|Y:{y}] - Index:{index} set to {_mapTemplate.Tiles[index]}");
            StateHasChanged();
        }

        private void CreateMapTemplate()
        {
            _mapTemplate = new MapTemplate { Height = 9, Width = 16, Name = "test", Tiles = new string('0', 16 * 9), BackgroundColor = "#241e3b" };
            Toast.ShowInfo("Empty map template created.");
            StateHasChanged();
        }

        private async Task AddMap()
        {
            var result = await Http.PostAsJsonAsync<MapTemplate>("api/game/addmap", _mapTemplate);
            _mapTemplate.Id = await result.Content.ReadFromJsonAsync<int>();

            _maps.Add(_mapTemplate.Id, _mapTemplate);
            _mapId = _mapTemplate.Id;
            Toast.ShowSuccess("Map successfully added.");
        }

        private async Task SaveMap()
        {
            var response = await Http.PutAsJsonAsync<MapTemplate>("api/game/upmap", _mapTemplate);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) {
                Toast.ShowSuccess("Map was removed in the meantime.");
                _maps.Remove(_mapTemplate.Id);

            } else {
                Toast.ShowSuccess("Map successfully modiffied.");

            }
        }
    }
}
