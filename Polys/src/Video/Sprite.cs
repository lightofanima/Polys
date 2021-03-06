﻿using System;
using OpenGL;

namespace Polys.Video
{
    public class Sprite : Transformable
    {
        /**  whether it is visible. */
        public bool  visible;

        int uvOffsetX, uvOffsetY;

        public Tileset tileset;

        /** Constructs a tile. The position is subtracted from the tile count to account for up=down issue.*/
        public Sprite(TiledSharp.TmxLayerTile tile, Tileset tileset, int genericTileWidth, int genericTileHeight, int tileCountY)
            : base(new Util.Rect(tile.X * genericTileWidth,
                (tileCountY - tile.Y - 1) * genericTileHeight,
                tileset.tileWidth,
                tileset.tileHeight))
        {
            visible = tile.Gid != 0;
            setUV(tile.Gid < 1 ? 0 : ((tile.Gid - tileset.firstGid) % tileset.tileCountX) * tileset.tileWidth,
                tile.Gid < 1 ? 0 : ((tile.Gid - tileset.firstGid) / tileset.tileCountX) * tileset.tileHeight);
            this.tileset = tileset;
        }

        /** Constructs the tile and loads a tileset */
        public Sprite(string spritePath, Util.Rect rect, bool originIsCentre = true,
            bool visible = true, int uvX = 0, int uvY = 0)
            : this(rect, new Tileset(spritePath, "tileset"), visible, uvX, uvY) { }

        /** Constructs the tile */
        public Sprite(Util.Rect rect, Tileset tileset, bool visible = true,
            int uvX = 0, int uvY = 0)
            : base(rect)
        {
            //Copy in properties
            this.visible = visible;
            setUV(uvX, uvY);
            this.tileset = tileset;
        }


        void setUV(int uvX, int uvY)
        {
            uvOffsetX = uvX;
            uvOffsetY = uvY;
        }

        public void setTilesetIndex(int x, int y)
        {
            uvOffsetX = x* rect.w;
            uvOffsetY = y* rect.h;
        }

        public Matrix4 uvMatrix(float tilesetWidth, float tilesetHeight)
        {
            return new Matrix4(new float[] { rect.w/tilesetWidth, 0, 0, 0,
                               0, rect.h/tilesetHeight, 0, 0,
                               0, 0, 0, 0,
                               uvOffsetX/tilesetWidth + (0.5f/tilesetWidth), uvOffsetY/tilesetHeight + (0.5f/tilesetHeight), 0, 1 });
        }
        
        public bool overlaps(Sprite s)
        {
            return rect.overlaps(s.rect);
        }
    }
}
