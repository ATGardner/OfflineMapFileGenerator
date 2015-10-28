﻿namespace com.atgardner.OMFG.sources
{
    using tiles;
    using System.Threading.Tasks;

    public interface ITileSource
    {
        Task<Tile> GetTileData(Tile tile);
    }
}
