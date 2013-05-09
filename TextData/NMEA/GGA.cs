/**************************************************************************
 * GGA.cs
 * NMEAのGGAセンテンスを処理するためのクラス for C#
 * 
 * 開発者　：森下功啓（K.Morhista Kumamoto-University）
 * 開発履歴：2011/7/3   GPS.cs整備開始
 *           2011/7/6   2地点間の距離を求める、GetDistanceのデバッグ。単位長までしか計算していなかった…。
 *           2011/7/17  XYZとBLHに二項演算子を追加
 *           2011/9/19  Open(string fname)のバグを修正
 *                      GetPositions(),GetPositioningResults()において、ubloxのNMEAフォーマットに対応（現時点での要求仕様を満たすだけなので万能ではないことに注意）
 *                      PositioningInfosのToString()に高度も含まれるように変更
 *           2011/11/11 よく分かっていないが、シリアル化した方が良さそうとのことで[Serializable]を片っ端からつけてみた。今後どうなるかは・・・よく分からない。
 *           2011/11/20 BLH構造体にToString()を追加
 *           2011/12/2  XML構文エラーがある部分を訂正した
 *           2011/12/15 BLH構造体にToString()を追加
 *           2012/4/22  コメントを少しだけ付け直した。
 *                      NMEAクラスは現時点ではバイナリの処理が不可能だが、そこんとこを何とかしたい。
 *           2012/4/24  GPS.csのソースコードを分割して独立させた。
 *           -------------------------------------------------------------
 *           2012/5/4   NMEA.csよりさらに分離。名前空間も変えた。
 *                      正規表現の準備を少しだけ進める。
 *           2012/5/5   正規表現の準備をさらに進める。
 *                      参考リンク：正規表現　http://msdn.microsoft.com/ja-jp/library/30wbz966(v=vs.80).aspx
 *                      テストは未実施なので注意のこと
 *           2012/5/6   センテンスの解析機能を取り急ぎ実装した。アルゴリズムは腐っているけど、とりあえず間に合わせ。
 *           2012/5/7   スタティックコンストラクタを整備して、エラー対策を実装した。
 *                      正規表現を利用したセンテンス解析を実施するように改めた。
 *           2012/5/22  静的メンバにGGA.を付けた。
 *           2012/5/28  Parse()のバグ取り
 * ***********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;   // for Regex

namespace GNSS.TextData.NMEA
{
    /// <summary>
    /// GGAのみを扱うクラス
    /// <para>未完成</para>
    /// <para>staticクラスとして宣言するので、インスタンスの確保を宣言せずとも使えます。</para>
    /// <para>南緯と西経は負数で表します。</para>
    /// </summary>
    public static class GGA
    {
        /*************** メンバ変数 ************************/
        /// <summary>
        /// GGAセンテンスにマッチさせる正規表現
        /// <para>NMEAのバージョンの違いをどうやって吸収するかはまだ未定</para>
        /// </summary>
        private static Regex regexMatch;
        /// <summary>
        /// GGAセンテンスにマッチさせて、内部の値をグループ化する正規表現
        /// <para>Pythonと違ってこちらだけでも用は足りるかもしれない。</para>
        /// </summary>
        private static Regex regexGroup;
        /*************** メソッド ************************/
        /// <summary>
        /// 文字列を検査してセンテンスにマッチするかどうかを確認する
        /// </summary>
        /// <param name="txt">被検査文字列</param>
        /// <returns>検査結果<para>true: ヒット</para></returns>
        public static Boolean CheckMatch(string txt)
        {
            Match m = GGA.regexMatch.Match(txt);
            return m.Success;
        }
        /// <summary>
        /// 検査文字列の中にあるマッチするものをすべて抽出する
        /// <para>Pythonのfindallを参考にした。</para>
        /// <para>テスト未実施</para>
        /// </summary>
        /// <param name="txt">被検査文字列</param>
        /// <returns>検査結果</returns>
        public static MatchCollection FindAll(string txt)
        {
            MatchCollection mc = GGA.regexMatch.Matches(txt);
            return mc;
        }
        /// <summary>
        /// セルフテスト
        /// <para>
        /// 本クラスのテストを実施する。
        /// 未実装。
        /// 返り値はHealthyかUnhealthyにするか、各メソッドへの入力と返り値を並べたstringとするか。。。</para>
        /// </summary>
        /// <returns></returns>
        public static Boolean Selftest()
        {
            return true;
        }
        /// <summary>
        /// GGAを解析して、測位情報を返す
        /// <para>現時点では緯度と経度と楕円体高しか返していないんだけど・・・</para>
        /// </summary>
        /// <param name="str">解析文字列</param>
        /// <returns></returns>
        public static PositioningInfo Parse(string str)
        {
            Blh pos = new Blh();                                // 初期値は全て0になっている
            PositioningInfo ans = new PositioningInfo();

            Match m = GGA.regexGroup.Match(str);                // マッチチェック
            if (m.Success) {
                string clock = m.Groups["clock"].Value;
                string lat = m.Groups["lat"].Value;
                string lon = m.Groups["lon"].Value;
                string alt_MSL = m.Groups["Alt_MSL"].Value;
                string alt_Geoid = m.Groups["Alt_Geoid"].Value;

                // 緯度経度と高度を取得する
                double _lat = 0.0, _lon = 0.0;
                if (lat != "") _lat = double.Parse(lat);
                if (_lat != 0.0) _lat = (double)((int)(_lat / 100.0d)) + (_lat % 100.0) / 60.0d;    // 単位を度へ変換する
                pos.B = _lat;
                if (m.Groups["NorS"].Value == "S") pos.B = -pos.B;                                  // 南緯は負数とする
                if (lon != "") _lon = double.Parse(lon);
                if (_lon != 0.0) _lon = (double)((int)(_lon / 100.0d)) + (_lon % 100.0) / 60.0d;
                pos.L = _lon;
                if (m.Groups["EorW"].Value == "W") pos.L = -pos.L;                                  // 西経は負数とする
                if (alt_MSL != "" && alt_Geoid != "") pos.H = double.Parse(alt_MSL) + double.Parse(alt_Geoid);// 平均海水面高とジオイド高を足して楕円体高とする
            }
            ans.Position = pos;
            return ans;
        }
        /// <summary>
        /// スタティックコンストラクタ
        /// <para>メンバの初期化を行います。</para>
        /// </summary>
        static GGA()
        {
            try
            {
                // 次の文字列にヒットすることは確認済み："$GPGGA,012211.00,3249.42677,N,13043.75860,E,1,08,0.99,113.3,M,29.0,M,,*5B"
                GGA.regexMatch = new Regex(@"\$GPGGA,\d+\.\d+,\d+\.\d+,[NS],\d+\.\d+,[EW],\d,\d+,\d+\.\d+,\d+\.\d,M,\d+\.\d,M,(?:\d+\.\d)?,\*");
                // Python用のコードのままだとエラーが出たので、グループ(?P<>)の中のPを削除した
                GGA.regexGroup = new Regex(@"(?<sentence>\$GPGGA,(?<clock>\d+\.\d+),(?<lat>\d+\.\d+),(?<NorS>[NS]),(?<lon>\d+\.\d+),(?<EorW>[EW]),(?<Fix_Mode>\d),(?<SVs_Used>\d+),(?<HDOP>\d+\.\d+),(?<Alt_MSL>\d+\.\d),M,(?<Alt_Geoid>\d+\.\d),M,(?:\d+\.\d)?,\*)");
            }
            catch 
            {
                throw new SystemException("GGAクラスのスタティックコンストラクタにおいて、Regex（正規表現）オブジェクトの生成に失敗しました。");
            }
        }
    }
}
