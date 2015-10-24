using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UOMachine;
using UOMachine.Data;
using UOMachine.Macros;
using UOMachine.Utility;

namespace UnitTests
{
    [TestClass]
    public class MapTests
    {
        private LandTileTest[] m_LandTileTests;

        [TestInitialize]
        public void Initialize()
        {
            m_LandTileTests = new LandTileTest[]
            {
                new LandTileTest(Facet.Felucca, 2637, 146, 3, "grass"),
                new LandTileTest(Facet.Trammel, 2637, 146, 3, "grass"),
                new LandTileTest(Facet.Ilshenar, 528, 216, 1089, "stone"),
                new LandTileTest(Facet.Malas, 2064, 1358, 628, "dirt"),
                new LandTileTest(Facet.Tokuno, 781, 1219, 3, "grass"),
                new LandTileTest(Facet.Ter_Mur, 1000, 500, 584, "cave"),
            };
        }

        [TestCategory("Data Format"),TestCategory("Data Format\\Map"), TestMethod]
        public void MapTest70022()
        {
            string path = @"D:\Clients\7.0.2.2";

            Version version = ExeInfo.GetFileVersion(Path.Combine(path, "client.exe"));

            TileData.Initialize(path);
            Map.Initialize(path, 0);

            MapInfo mi;
            foreach (LandTileTest lt in m_LandTileTests)
            {
                MacroEx.GetMapInfo(lt.Facet, lt.X, lt.Y, out mi);
                Assert.IsNotNull(mi.landTile, "LandTile is null.");
                Assert.AreEqual(mi.landTile.ID, lt.LandTileID, "LandTile doesn't match expected.");
                Assert.AreEqual(mi.landTile.Name, lt.LandTileName, "LandTile Name doesn't match expected.");
            }

            TileData.Dispose();
            Map.Dispose();
        }

        [TestCategory("Data Format"), TestCategory("Data Format\\Map"),TestMethod]
        public void MapTest70200()
        {
            string path = @"D:\Clients\7.0.20.0";

            TileData.Initialize(path);
            Map.Initialize(path, 0);

            MapInfo mi;
            foreach (LandTileTest lt in m_LandTileTests)
            {
                MacroEx.GetMapInfo(lt.Facet, lt.X, lt.Y, out mi);
                Assert.IsNotNull(mi.landTile, "LandTile is null.");
                Assert.AreEqual(mi.landTile.ID, lt.LandTileID, "LandTile doesn't match expected.");
                Assert.AreEqual(mi.landTile.Name, lt.LandTileName, "LandTile Name doesn't match expected.");
            }

            TileData.Dispose();
            Map.Dispose();
        }

        [TestCategory("Data Format"), TestCategory("Data Format\\Map"),TestMethod]
        public void MapTest70351()
        {
            string path = @"D:\Clients\7.0.35.1";

            TileData.Initialize(path);
            Map.Initialize(path, 0);

            MapInfo mi;
            foreach (LandTileTest lt in m_LandTileTests)
            {
                MacroEx.GetMapInfo(lt.Facet, lt.X, lt.Y, out mi);
                Assert.IsNotNull(mi.landTile, "LandTile is null.");
                Assert.AreEqual(mi.landTile.ID, lt.LandTileID, "LandTile doesn't match expected.");
                Assert.AreEqual(mi.landTile.Name, lt.LandTileName, "LandTile Name doesn't match expected.");
            }

            TileData.Dispose();
            Map.Dispose();
        }

        [TestCategory("Data Format"), TestCategory("Data Format\\Map"),TestMethod]
        public void MapTest70450()
        {
            string path = @"D:\Clients\7.0.45.0";

            TileData.Initialize(path);
            Map.Initialize(path, 0);

            MapInfo mi;
            foreach (LandTileTest lt in m_LandTileTests)
            {
                MacroEx.GetMapInfo(lt.Facet, lt.X, lt.Y, out mi);
                Assert.IsNotNull(mi.landTile, "LandTile is null.");
                Assert.AreEqual(mi.landTile.ID, lt.LandTileID, "LandTile doesn't match expected.");
                Assert.AreEqual(mi.landTile.Name, lt.LandTileName, "LandTile Name doesn't match expected.");
            }

            TileData.Dispose();
            Map.Dispose();
        }

        [TestCategory( "Data Format" ), TestCategory( "Data Format\\Map" ), TestMethod]
        public void MapTest70462()
        {
            string path = @"D:\Clients\7.0.46.2";

            TileData.Initialize( path );
            Map.Initialize( path, 0 );

            MapInfo mi;
            foreach (LandTileTest lt in m_LandTileTests)
            {
                MacroEx.GetMapInfo( lt.Facet, lt.X, lt.Y, out mi );
                Assert.IsNotNull( mi.landTile, "LandTile is null." );
                Assert.AreEqual( mi.landTile.ID, lt.LandTileID, "LandTile doesn't match expected." );
                Assert.AreEqual( mi.landTile.Name, lt.LandTileName, "LandTile Name doesn't match expected." );
            }

            TileData.Dispose();
            Map.Dispose();
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        public class LandTileTest
        {
            private Facet m_Facet;
            private int m_X;
            private int m_Y;
            private int m_LandTileID;
            private string m_LandTileName;

            public Facet Facet
            {
                get
                {
                    return m_Facet;
                }
            }

            public int X
            {
                get
                {
                    return m_X;
                }
            }

            public int Y
            {
                get
                {
                    return m_Y;
                }
            }

            public int LandTileID
            {
                get
                {
                    return m_LandTileID;
                }
            }

            public string LandTileName
            {
                get
                {
                    return m_LandTileName;
                }
            }

            public LandTileTest(Facet facet, int x, int y, int landtileid, string landtilename)
            {
                m_Facet = facet;
                m_X = x;
                m_Y = y;
                m_LandTileID = landtileid;
                m_LandTileName = landtilename;
            }
        }
    }
}
