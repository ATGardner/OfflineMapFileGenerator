﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace com.atgardner.Downloader
{
    public static class Downloader
    {
        public static event EventHandler<DownloadTileEventArgs> TileDownloaded;
        private static readonly Regex subDomainRegExp = new Regex(@"\[(.*)\]");
        private static int subDomainNum;
        private static string subDomains;

        public static void DownloadTiles(IEnumerable<Coordinate> coordinates, int[] zoomLevels, string sourceName, string addressTemplate)
        {
            subDomainNum = 0;
            var tiles = GenerateTiles(coordinates, zoomLevels);
            var set = new Dictionary<Tile, Task>();
            foreach (var tile in tiles)
            {
                FireTileDownloaded(tile, DownloadPhase.Ready);
                if (set.ContainsKey(tile) || set.Count > 10)
                {
                    FireTileDownloaded(tile, DownloadPhase.Skipped);
                }
                else
                {
                    set[tile] = DownloadTileAsync(tile, sourceName, addressTemplate);
                }
            }

            Task.WaitAll(set.Values.ToArray());
            FireTileDownloaded(null, DownloadPhase.Complete);
        }

        private static IEnumerable<Tile> GenerateTiles(IEnumerable<Coordinate> coordinates, int[] zoomLevels)
        {
            foreach (var c in coordinates)
            {
                var lon = c.Longitude;
                var lat = c.Latitude;
                foreach (var zoom in zoomLevels)
                {
                    yield return WorldToTilePos(lon, lat, zoom);
                }
            }
        }

        private static async Task<Tile> DownloadTileAsync(Tile tile, string source, string addressTemplate)
        {
            FireTileDownloaded(tile, DownloadPhase.Started);
            var address = GetAddress(addressTemplate, tile);
            var ext = Path.GetExtension(address);
            var fileName = string.Format("{0}/{1}/{2}/{3}{4}", source, tile.Zoom, tile.X, tile.Y, ext);
            if (File.Exists(fileName))
            {
                FireTileDownloaded(tile, DownloadPhase.TileDone);
                return tile;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            await PerformTask(address, fileName);
            FireTileDownloaded(tile, DownloadPhase.TileDone);
            return tile;
        }

        private static async Task PerformTask(string address, string fileName)
        {
            //var webClient = new WebClient();
            //await webClient.DownloadFileTaskAsync(address, fileName);
            (new Thread(() => {
                Thread.Sleep(100);
                Console.WriteLine("done downloadig {0}", address);
            })).Start();
        }

        private static void FireTileDownloaded(Tile tile, DownloadPhase phase)
        {
            var handler = TileDownloaded;
            var args = new DownloadTileEventArgs(tile, phase);
            if (handler != null)
            {
                handler(null, args);
            }
        }

        private static Tile WorldToTilePos(double lon, double lat, int zoom)
        {
            var x = (int)((lon + 180.0) / 360.0 * (1 << zoom));
            var y = (int)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) + 1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));
            return new Tile(x, y, zoom);
        }

        private static string GetAddress(string addressTemplate, Tile tile)
        {
            var match = subDomainRegExp.Match(addressTemplate);
            if (match.Success)
            {
                var subDomain = match.Groups[1].Value;
                var currentSubDomain = subDomain.Substring(subDomainNum, 1);
                subDomainNum = (subDomainNum + 1) % subDomain.Length;
                addressTemplate = subDomainRegExp.Replace(addressTemplate, currentSubDomain);
            }

            return addressTemplate.Replace("{z}", "{zoom}").Replace("{zoom}", tile.Zoom.ToString()).Replace("{x}", tile.X.ToString()).Replace("{y}", tile.Y.ToString());
        }
    }
}