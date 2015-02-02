﻿namespace com.atgardner.OMFG.packagers
{
    using com.atgardner.OMFG.tiles;
    using System;
    using System.Threading.Tasks;

    interface IPackager : IDisposable
    {
        Task Init();

        Task AddTile(Tile tile);
    }
}
