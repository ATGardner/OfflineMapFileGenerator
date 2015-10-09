﻿namespace com.atgardner.OMFG.sources
{
    using com.atgardner.OMFG.tiles;
    using utils;
    using NLog;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    class MBTilesSource : ITileSource, IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly string SELECT_SQL = "select tile_data from tiles where tile_column = @tile_column and tile_row = @tile_row and zoom_level = @zoom_level;";
        private bool initialized;
        private readonly Database database;

        public MBTilesSource(string address)
        {
            database = new Database(address);
        }

        public void Init()
        {
            database.Open();
        }

        public async Task<Tile> GetTileData(Tile tile)
        {
            if (!initialized)
            {
                Init();
                initialized = true;
            }

            //switching the tile_row direction
            var y = (1 << tile.Zoom) - tile.Y - 1;
            tile.Image = (byte[])await database.ExecuteScalarAsync(SELECT_SQL, new Dictionary<string, object> { { "tile_column", tile.X }, { "tile_row", y }, { "zoom_level", tile.Zoom } });
            if (!tile.HasData)
            {
                
            }

            return tile;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    database.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
