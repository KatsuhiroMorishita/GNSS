/**************************************************************************
 * GnssTest.cs
 * GNSS名前空間に属するクラス群をテストする静的クラス for C#
 * 
 * 開発者　：森下功啓（K.Morhista Kumamoto-University）
 * 開発履歴：
 *          2012/5/19   開発開始
 *          2012/5/28   ReadNmeaToRectangleFeild()を新設した。
 *          2012/6/5    TwoBlhToDistanceTest()を新設した。
 *          2012/7/3    コメントをわずかに変更した。
 *                      また、「八代高専」を「熊本高専八代キャンパス」へ変更した。嗚呼、統合されしわが母校。
 * ***********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GNSS.Field;
using GNSS.TextData.NMEA;

namespace GNSS
{
    /// <summary>
    /// GNSS名前空間に属するクラス群をテストする静的クラス
    /// </summary>
    public static class GnssTest
    {
        /// <summary>
        /// 2地点間の距離を求めるテスト
        /// <para>コンソールアプリでの利用を前提としています。</para>
        /// <para>ソースコードのコメントにおける、Google Earthの数字はver. 6.1.0.5001にて確認したものです。</para>
        /// <para>Google Earthとの差は、計算アルゴリズム若しくは測地系に起因するものと考えられます。（未チェック）</para>
        /// </summary>
        public static void TwoBlhToDistanceTest()
        {
            Console.WriteLine("Test 1: 熊本高専八代キャンパスで地図上（カシミール3D）で約10 mの距離");
            Blh x1 = new Blh(32.476667, 130.607132);    // 八代高専で地図上（カシミール3D）で約10 mの距離
            Blh x2 = new Blh("32.476757", "130.607132");// 文字列でもよい
            Console.WriteLine("test point is " + x1.ToString() + " and " + x2.ToString());
            Console.WriteLine("GetDistance2 test result: " + x1.GetDistance2(x2).ToString());
            Console.WriteLine("GetDistance3 test result: " + x1.GetDistance3(x2).ToString());
            Console.WriteLine("GetDistance4 test result: " + x1.GetDistance4(x2).ToString());
            Console.WriteLine("GetDistance5 test result: " + x1.GetDistance5(x2).ToString());
            Console.WriteLine("GetDistance6 test result: " + x1.GetDistance6(x2).ToString());

            Console.WriteLine("Test 2: 熊本高専八代キャンパス－熊本大学");
            x1 = new Blh(32.476032, 130.606439);    // 八代高専－熊本大学，（Google Earthにおいて）地図上39,131.83, 地上39,131.69, 方位197.02 ただし、八代高専は標高4 m、熊大は標高27 mとされていた。
            x2 = new Blh(32.813365, 130.728803);
            Console.WriteLine("test point is " + x1.ToString() + " and " + x2.ToString());
            Console.WriteLine("GetDistance2 test result: " + x1.GetDistance2(x2).ToString());
            Console.WriteLine("GetDistance3 test result: " + x1.GetDistance3(x2).ToString());
            Console.WriteLine("GetDistance4 test result: " + x1.GetDistance4(x2).ToString());
            Console.WriteLine("GetDistance5 test result: " + x1.GetDistance5(x2).ToString());
            Console.WriteLine("GetDistance6 test result: " + x1.GetDistance6(x2).ToString());

            Console.WriteLine("Test 3: 熊本大学－東京海洋大学");
            x1 = new Blh(32.813365, 130.728803);    // 熊本大学－東京海洋大学，（Google Earthにおいて）地図上892,360.80, 地上892,362.87, 方位66.63 ただし、東京海洋大学は標高2 mとされていた。
            x2 = new Blh(35.667092, 139.790611);
            Console.WriteLine("test point is " + x1.ToString() + " and " + x2.ToString());
            Console.WriteLine("GetDistance2 test result: " + x1.GetDistance2(x2).ToString());
            Console.WriteLine("GetDistance3 test result: " + x1.GetDistance3(x2).ToString());
            Console.WriteLine("GetDistance4 test result: " + x1.GetDistance4(x2).ToString());
            Console.WriteLine("GetDistance5 test result: " + x1.GetDistance5(x2).ToString());
            Console.WriteLine("GetDistance6 test result: " + x1.GetDistance6(x2).ToString());

            Console.WriteLine("Test 4: 東京海洋大学－サンフランシスコ");
            x1 = new Blh(35.667092, 139.790611);    // 東京海洋大学－サンフランシスコ，（Google Earthにおいて）地図上8,283,703.36, 地上8,283,715.04, 方位55.43 ただし、サンフランシスコは標高10 mとされていた。
            x2 = new Blh(37.774931, -122.419414);
            Console.WriteLine("test point is " + x1.ToString() + " and " + x2.ToString());
            Console.WriteLine("GetDistance2 test result: " + x1.GetDistance2(x2).ToString());
            Console.WriteLine("GetDistance3 test result: " + x1.GetDistance3(x2).ToString());
            Console.WriteLine("GetDistance4 test result: " + x1.GetDistance4(x2).ToString());
            Console.WriteLine("GetDistance5 test result: " + x1.GetDistance5(x2).ToString());
            Console.WriteLine("GetDistance6 test result: " + x1.GetDistance6(x2).ToString());
            return;
        }
        /// <summary>
        /// NMEA読み出し機能およびRectangleFieldクラスのテスト
        /// <para>ダイアログを利用してNMEAファイルを開きます。</para>
        /// <para>NMEA読み取り後に、測位結果を配列化して測位結果全体が収まるRectangleFieldオブジェクトを生成します。</para>
        /// <para>未測位データは除いています。</para>
        /// </summary>
        /// <returns>測位データ全体を覆う長方形領域情報</returns>
        public static RectangleField ReadNmeaToRectangleFeild()
        {
            NmeaReader reader = new NmeaReader();
            reader.OpenWithDialog();                            // NMEAファイル読み出し
            Blh[] pos = reader.GetPositions();                  // 測位情報のうちから位置座標のみを取得
            RectangleField field = new RectangleField();
            foreach (Blh x in pos)                              // 最外領域を求める
            {
                if (x.B != 0.0 && x.L != 0.0) field.Set(x);     // 未測位データは除く
            }
            return field;
        }
    }
}
