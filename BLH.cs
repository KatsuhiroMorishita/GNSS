/**************************************************************************
 * Blh.cs
 * 緯度経度表現の座標のための構造体 for C#
 * 
 * 開発者　：森下功啓（K.Morhista Kumamoto-University）
 * 開発履歴：
 *          2011/7/3    整備開始
 *          2011/7/6    2地点間の距離を求める、GetDistanceのデバッグ。単位長までしか計算していなかった…。
 *          2011/7/17   XYZとBLHに二項演算子を追加
 *          2011/9/19   Open(string fname)のバグを修正
 *                      GetPositions(),GetPositioningResults()において、ubloxのNMEAフォーマットに対応（現時点での要求仕様を満たすだけなので万能ではないことに注意）
 *                      PositioningInfosのToString()に高度も含まれるように変更
 *          2011/11/11  よく分かっていないが、シリアル化した方が良さそうとのことで[Serializable]を片っ端からつけてみた。今後どうなるかは・・・よく分からない。
 *          2011/11/20  BLH構造体にToString()を追加
 *          2011/12/2   XML構文エラーがある部分を訂正した
 *          2011/12/15  BLH構造体にToString()を追加
 *          2012/4/22   コメントを少しだけ付け直した。
 *                      NMEAクラスは現時点ではバイナリの処理が不可能だが、そこんとこを何とかしたい。
 *          2012/4/24   ソースコードを分割した。
 *                      プロパティになっていて不自然な感じがするものはメソッドへ変更した。
 *                      測地系を変更できるような準備を導入した。が、発展させるかは不明。。。
 *                      ------------------------------------------------------------------------
 *          2012/4/29   Equals()を実装した。
 *          2012/5/11   ToXYZ()に、変換後の測地系を省略できるバージョンを整備した。
 *                      FilterAsLatitudeAndLongitude()を整備して、座標同士の足し算・引き算で極や経度180°をまたいだ演算を安全に行えるようにした。
 *          2012/5/14   2地点間の中間座標を返すGetMedian()に有ったバグを修正した。
 *                      以前は経度の中点が球面上に2点あることを考慮していなかった。
 *                      プロパティにHealthyを追加した。
 *                      以上の2点及び11日に追加した分の一応のテストは実施済み。
 *          2012/5/21   コメントを少しだけ見直した。
 *                      == の定義に測地系の条件も含めた。
 *          2012/6/5    一部のコメントを修正
 *                      簡易距離計算メソッドGetDistance4()を新設
 *                      GetDistance()が東経と西経間の距離を正常に算出できないバグがあったので修正した。それに伴い、これを利用していたGetDistance3()のバグも治った。
 *                      より長距離で使えるGetDistance5()とGetDistance6()（ただし、実質同じアルゴリズム）を整備した。
 *          2012/7/1    BLHを文字列で初期化するコンストラクタを整備
 *          2012/7/3    測地系の変換に対する対応の一環として、WGS84クラスの設計変更への対応を実施した。
 *          2012/7/23   GRS80測地系へ対応
 *          2012/8/6    GlobalDatumクラスを新設したので、対応させた。
 *                      GetUnitLengthForEN()メソッドは引数に楕円体の定義を渡せるようになっていたが、実質的に意味がないので廃止した。
 *          2012/8/9    不必要な名前空間をusingから削除
 *          2012/10/9   XMLのコメントを一部修正
 *          2012/10/10  旧体育の日。今日は晴れ。クラスの名前をBLHからBlhへ変更した。
 * ***********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Geodesy.GeodeticDatum;

namespace GNSS
{
    /*2011/7/6 動作確認済み*/
    /// <summary>
    /// 緯度・経度・楕円体高を表す構造体
    /// <para>語源はドイツ語らしい。</para>
    /// </summary>
    public struct Blh
    {
        /* メンバ変数 ***************************************************/
        /// <summary>
        /// 緯度
        /// </summary>
        private double b;
        /// <summary>
        /// 経度
        /// </summary>
        private double l;
        /// <summary>
        /// 楕円体高[m]
        /// </summary>
        private double h;
        /// <summary>
        /// 緯度・経度の単位
        /// </summary>
        private AngleUnit _unit;
        /// <summary>
        /// 測地系
        /// </summary>
        private Datum _datum;
        /* 演算子 ***************************************************/
        /// <summary>
        /// 二項+演算子（これで足し算が簡単にできる）
        /// <para>単位はdegreeとなる</para>
        /// </summary>
        /// <param name="c1">被加算値</param>
        /// <param name="c2">加算値</param>
        /// <returns>2値を加算した結果</returns>
        public static Blh operator +(Blh c1, Blh c2)
        {
            return new Blh(c1.ToDegree().B + c2.ToDegree().B, c1.ToDegree().L + c2.ToDegree().L, c1.ToDegree().H + c2.ToDegree().H, AngleUnit.Degree);
        }
        /// <summary>
        /// 二項-演算子（これで引き算が簡単にできる）
        /// <para>単位はdegreeとなる</para>
        /// </summary>
        /// <param name="c1">被減算値</param>
        /// <param name="c2">減算値</param>
        /// <returns>2値の引き算の結果</returns>
        public static Blh operator -(Blh c1, Blh c2)
        {
            return new Blh(c1.ToDegree().B - c2.ToDegree().B, c1.ToDegree().L - c2.ToDegree().L, c1.ToDegree().H - c2.ToDegree().H, AngleUnit.Degree);
        }
        /// <summary>
        /// 比較演算子==
        /// <para>単位及び測地系の設定も含めて一致しなければtrueとはしません。</para>
        /// </summary>
        /// <param name="c1">比較対象その1</param>
        /// <param name="c2">比較対象その2</param>
        /// <returns>等しければture</returns>
        public static bool operator ==(Blh c1, Blh c2)
        {
            if (object.ReferenceEquals(c1, c2))                 // 両方nullか（参照元が同じか）チェックする。(c1 == c2)とすると、無限ループ
            {
                return true;
            }
            if (((object)c1 == null) || ((object)c2 == null))   // どちらかがnullかチェックする。(c1 == null)とすると、無限ループ
            {
                return false;
            }
            return (c1.B == c2.B) && (c1.L == c2.L) && (c1.H == c2.H) && (c1._unit == c2._unit) && (c1._datum == c2._datum);
        }
        /// <summary>
        /// 比較演算子!=
        /// </summary>
        /// <param name="c1">比較対象その1</param>
        /// <param name="c2">比較対象その2</param>
        /// <returns>等しくなければture</returns>
        public static bool operator !=(Blh c1, Blh c2)
        {
            return !(c1 == c2);                 // (c1 != c2)とすると、無限ループ
        }
        /// <summary>
        /// ハッシュ値を返す
        /// </summary>
        /// <returns>全メンバのXOR結果</returns>
        public override int GetHashCode()
        {
            return this.B.GetHashCode() ^ this.L.GetHashCode() ^ this.H.GetHashCode() ^ this._unit.GetHashCode();
        }
        /// <summary>
        /// objと自分自身が等価のときはtrueを返す
        /// </summary>
        /// <param name="obj">比較したいオブジェクト</param>
        /// <returns>等価であればtrue</returns>
        public override bool Equals(object obj)
        {
            //objがnullか、型が違うときは、等価でない
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }
            //この型が継承できないクラスや構造体であれば、次のようにできる
            //if (!(obj is TestClass))

            // メンバで比較する
            Blh c = (Blh)obj;
            return (this.B == c.B && this.L == c.L && this.H == c.H && this.Unit == c.Unit && this.DatumKind == c.DatumKind);
        }
        /* プロパティ ***********************************************/
        /// <summary>
        /// 緯度
        /// <para>緯度は-90～90 degの範囲に丸められます。</para>
        /// <para>もしセットした値の絶対値が90°を超えている場合、極の向こう側へ移動したとみなして経度が自動的に半周します。</para>
        /// </summary>
        public double B
        {
            get { return this.b; }
            set {
                this.b = value;
                Blh.FilterAsLatitudeAndLongitude(ref this.b, ref this.l, this.Unit);
            }
        }
        /// <summary>
        /// 経度
        /// <para>経度は-180～180 degの範囲に丸められます。</para>
        /// </summary>
        public double L
        {
            get { return this.l; }
            set {
                this.l = value;
                Blh.FilterAsLongitude(ref this.l, this.Unit);
            }
        }
        /// <summary>
        /// 楕円体高
        /// <para>楕円体表面からの鉛直距離</para>
        /// </summary>
        public double H
        {
            get { return this.h; }
            set {
                double _h = value;                  // 地心よりも深い値を指定されてもそれ以上いかないようにする。地球の反対側に向かっていかせてもいいけど…。
                if (_h < -6400000) _h = -6400000;   // ほんとは-赤道半径が良いんだろうなぁ。
                this.h = _h; 
            }
        }
        /// <summary>
        /// 演算単位
        /// <para>deg or radian</para>
        /// </summary>
        public AngleUnit Unit
        {
            get 
            {
                return this._unit;
            }
            set
            {
                if (value == AngleUnit.Degree && this._unit == AngleUnit.Radian)
                {
                    this.ChangeToDegree();
                }
                else if (value == AngleUnit.Radian && this._unit == AngleUnit.Degree)
                {
                    this.ChangeToRadian();
                }
            }
        }
        /// <summary>
        /// 測地系
        /// <para>測地系の変更に対しての処理は未実装</para>
        /// </summary>
        public Datum DatumKind
        {
            get
            {
                return this._datum;
            }
            set {
                this._datum = value;
            }
        }
        /// <summary>
        /// 本オブジェクトの状態を表す
        /// <para>true: 正常, 利用可能</para>
        /// </summary>
        public Boolean Healthy
        {
            get {
                if (double.IsNaN(this.b) || double.IsNaN(this.l) || double.IsNaN(this.h))
                    return false;
                else
                    return true;
            }
        }
        /* メソッド ***********************************************/
        /// <summary>
        /// 単位を度へ変換したオブジェクトを返す
        /// </summary>
        public Blh ToDegree()
        {
            Blh ans = this;

            if (this._unit == AngleUnit.Radian)
            {
                ans.B = this.B * 180.0d / Math.PI;
                ans.L = this.L * 180.0d / Math.PI;
                ans._unit = AngleUnit.Degree;
            }
            return ans;
        }
        /// <summary>
        /// 単位をラジアン単位へ変換したオブジェクトを返す
        /// </summary>
        public Blh ToRadian()
        {
            Blh ans = this;

            if (this._unit == AngleUnit.Degree)
            {
                ans.B = this.B * Math.PI / 180.0d;
                ans.L = this.L * Math.PI / 180.0d;
                ans._unit = AngleUnit.Radian;
            }
            return ans;
        }
        /// <summary>
        /// 楕円体に沿った、東西方向の単位長[m/deg]を返す
        /// <para>実際は高度の分だけ若干の誤差が発生するし、2地点の高度差は考慮しない。</para>
        /// <para>参考：理科年表，p.563，2003．</para>
        /// <para>ちなみに、http://yamadarake.web.fc2.com/trdi/2009/report000001.html　を見ると使用した公式がヒュベニの公式と言うものであることが分かる</para>
        /// </summary>
        /// <returns>楕円体に沿った、東西方向の単位長[m/deg]</returns>
        public Length GetUnitLengthForEN()
        {
            Length ans = new Length(0, 0);
            Blh temp = this.ToRadian();
            GlobalDatum _datum = new GlobalDatum(this.DatumKind);

            ans.E = Math.PI / 180.0 * _datum.a * Math.Cos(temp.B) / Math.Sqrt(1.0d - _datum.e2 * Math.Pow(Math.Sin(temp.B), 2.0d));   // 高度については無視
            ans.N = Math.PI / 180.0 * _datum.a * (1.0d - _datum.e2) / Math.Pow(1.0d - _datum.e2 * Math.Pow(Math.Sin(temp.B), 2.0d), 1.5d); // 地表面に限れば誤差は無視可能。誤差は、地上では最大6408/6400程度
            return ans;
        }
        /// <summary>
        /// 自身の単位を度へ変換する
        /// </summary>
        /// 
        private void ChangeToDegree()
        {
            if (this._unit == AngleUnit.Radian)
            {
                this.B = this.B * 180.0 / Math.PI;
                this.L = this.L * 180.0 / Math.PI;
                this._unit = AngleUnit.Degree;
            }
            return;
        }
        /// <summary>
        /// 自身の単位をラジアン単位へ変換する
        /// </summary>
        private void ChangeToRadian()
        {
            if (this._unit == AngleUnit.Degree)
            {
                this.B = this.B * Math.PI / 180.0;
                this.L = this.L * Math.PI / 180.0;
                this._unit = AngleUnit.Radian;
            }
            return;
        }
        /// <summary>
        /// 2地点間の中間座標を返す
        /// <para>（現時点では）メルカトル図法上の中間点を算出します。従って極点をまたがるような2点間の中点を求める用途には向いていません。</para>
        /// <para>また、2地点間の中間地点は地球上に2点取り得ますが、2点間を結ぶ距離が短くなる方を採用しています。</para>
        /// </summary>
        /// <param name="pos">第2の座標</param>
        /// <returns>
        /// 本インスタンスと第2の座標間の中間座標
        /// <para>新しく生成する座標の単位と測地系は本オブジェクトに合わせます。</para>
        /// </returns>
        public Blh GetMedian(Blh pos)
        {
            Blh me = this;                              // 自身のコピー
            Blh he = pos;                               // 引数のコピー
            he.Unit = this.Unit;                        // 単位は統一する
            he.DatumKind = this.DatumKind;              // 測地系も統一する

            Blh relative = he - me;                     // me基準の相対座標を計算
            double centerLon = me.L + relative.L / 2.0; // 2地点の中央の経度を計算（この時点ではAbs(180.0又はπ)を超えるかもしれない）

            return new Blh((me.B + he.B) / 2.0d, centerLon, (me.H + he.H) / 2.0d, this.Unit, this.DatumKind);   // 2地点の中間座標を求める
        }
        /// <summary>
        /// 距離演算その1
        /// <para>ヒュベニの公式を利用して、引数で指定された座標までの距離[m]を返します。</para>
        /// <para>GetDistance2()と比較して、40 km差で0.01 m以下の差が生じます。</para>
        /// <para>最短距離を求めているわけではないことに注意してください。</para>
        /// <para>なお、高度は無視して楕円体面上での距離を求めています。</para>
        /// </summary>
        /// <param name="pos">求めたい地点の座標</param>
        /// <returns>南北・東西方向の距離[m]を構造体で返す</returns>
        public Length GetDistance(Blh pos)
        {
            Length ans;
            Blh me = this.ToDegree();                   // 自身のコピー
            Blh he = pos.ToDegree();                    // 引数のコピー。単位はdegに統一する
            Field.RectangleField field = new Field.RectangleField(me, he);
            Blh center = field.Center;

            ans = new Length(field.DifferInLon * center.GetUnitLengthForEN().E, field.DifferInLat * center.GetUnitLengthForEN().N); // 緯度・経度差から距離を求める。
            return ans;
        }
        /// <summary>
        /// 距離演算その2
        /// <para>
        /// <para>引数で指定された座標までの距離[m]を返します。</para>
        /// 参考：http://homepage3.nifty.com/kubota01/distance.htm
        /// 距離が50kmを超えるようなら、こちらのメソッドの使用を推奨します。
        /// ただし、数百km以上であればその5またはその6の使用を推奨します。
        /// <para>なお、高度は無視して楕円体面上での距離を求めています。</para>
        /// </para>
        /// </summary>
        /// <param name="pos">求めたい地点の座標</param>
        /// <returns>距離</returns>
        public double GetDistance2(Blh pos)
        {
            Ecef me = this.ToXYZ();
            Ecef he = pos.ToXYZ();
            GlobalDatum _datum = new GlobalDatum(this.DatumKind);

            double d_straight = Math.Sqrt(Math.Pow(me.x - he.x, 2.0) + Math.Pow(me.y - he.y, 2.0) + Math.Pow(me.z - he.z, 2.0));   // XYZ直交座標系を用いた直線距離（線は地中に潜る）
            double Nme = _datum.a / Math.Sqrt(1.0d - _datum.e2 * Math.Sin(this.ToRadian().B));
            double Nse = _datum.a / Math.Sqrt(1.0d - _datum.e2 * Math.Sin(pos.ToRadian().B));
            double N = (Nse + Nme) / 2.0d;                                                         // 平均の半径のようなもの
            double angle = Math.Asin(d_straight / 2.0d / N);                                       // 半射程角（絵を描けば分かる）
            return 2.0d * angle * N;
        }
        /// <summary>
        /// 距離演算その3
        /// <para>ヒュベニの公式を利用しているGetDistance()を用いて、楕円体面上の距離[m]を求めます。</para>
        /// <para>精度は実距離100 km当たり、1 m程度です。</para>
        /// <para>なお、高度は無視して楕円体面上での距離を求めています。</para>
        /// </summary>
        /// <param name="pos">求めたい地点の座標</param>
        /// <returns>距離</returns>
        public double GetDistance3(Blh pos)
        {
            Length distance0 = this.GetDistance(pos);
            double distance3 = Math.Sqrt(distance0.E * distance0.E + distance0.N * distance0.N);
            return distance3;
        }
        /// <summary>
        /// 距離演算その4
        /// <para>
        /// 2点をXYZ座標へ変換して、そのベクトルの差分のノルムを取る（== 直線距離[m]）という処理を行います。
        /// 40 kmでその2に対して0.1 m以下の差を生じる。
        /// 地表における距離10 km以下であれば実用上は問題はないと考えられる。
        /// 演算の速さが取り柄です。
        /// 標高も演算に使われます。
        /// また、標高が高い（高度数十km）地点での短距離はこのメソッドの方がその2や3よりも精度上優位でしょう。
        /// </para>
        /// </summary>
        /// <param name="pos">求めたい地点の座標</param>
        /// <returns>距離</returns>
        public double GetDistance4(Blh pos)
        {
            Ecef me = this.ToXYZ();
            Ecef he = pos.ToXYZ();
            return (me - he).Norm;
        }
        /// <summary>
        /// 距離演算その5
        /// <para>Lambert-Andoyerの式を用いて引数で指定された座標までの距離[m]を返します。</para>
        /// <para>10 m程度の距離で0.2 mm程度の誤差を生じます。数百km以上の長距離になると誤差は少なめです。</para>
        /// <para>距離演算その6と比較して、10 m程のごく短距離において、0.01 pm（ピコメートル）の差が生じました。また、通常距離では差は生じませんでした。</para>
        /// <para>参考文献</para>
        /// <para>[1] 河合，測地線航海算法，富山高専　航海科学研究室，http://www.toyama-cmt.ac.jp/~mkawai/lecture/sailing/geodetic/geosail.html#note1 ，2012/6．</para>
        /// </summary>
        /// <param name="pos">求めたい地点の座標</param>
        /// <returns>測地線長<para>未知の測地系の場合、非値を返します。</para></returns>
        public double GetDistance5(Blh pos)
        {
            Blh me = this.ToRadian();
            Blh he = pos.ToRadian();
            GlobalDatum _datum = new GlobalDatum(this.DatumKind);

            double myParametricLat = Math.Atan(_datum.b / _datum.a * Math.Tan(me.B));
            double hisParametricLat = Math.Atan(_datum.b / _datum.a * Math.Tan(he.B));
            double sphericalDistance = Math.Acos(Math.Sin(myParametricLat) * Math.Sin(hisParametricLat) + Math.Cos(myParametricLat) * Math.Cos(hisParametricLat) * Math.Cos(me.L - he.L));
            double C = Math.Pow(Math.Sin(myParametricLat) + Math.Sin(hisParametricLat), 2.0);
            double D = Math.Pow(Math.Sin(myParametricLat) - Math.Sin(hisParametricLat), 2.0);
            double correction = _datum.f / 8.0 *
                (
                (Math.Sin(sphericalDistance) - sphericalDistance) * C / Math.Pow(Math.Cos(sphericalDistance / 2.0), 2.0)
                - (Math.Sin(sphericalDistance) + sphericalDistance) * D / Math.Pow(Math.Sin(sphericalDistance / 2.0), 2.0)
                );
            double geodeticDistance = double.NaN;
            geodeticDistance = _datum.a * (sphericalDistance + correction);

            return geodeticDistance;
        }
        /// <summary>
        /// 距離演算その6
        /// <para>Lambert-Andoyerの式を変形した小野の公式を用い引数で指定された座標までの距離[m]を返します。</para>
        /// <para>参考文献</para>
        /// <para>[1] 河合，測地線航海算法，富山高専　航海科学研究室，http://www.toyama-cmt.ac.jp/~mkawai/lecture/sailing/geodetic/geosail.html#note1 ，2012/6．</para>
        /// <para></para>
        /// </summary>
        /// <param name="pos">求めたい地点の座標</param>
        /// <returns>測地線長<para>未知の測地系の場合、非値を返します。</para></returns>
        public double GetDistance6(Blh pos)
        {
            Blh me = this.ToRadian();
            Blh he = pos.ToRadian();
            GlobalDatum _datum = new GlobalDatum(this.DatumKind);

            double myParametricLat = Math.Atan(_datum.b / _datum.a * Math.Tan(me.B));
            double hisParametricLat = Math.Atan(_datum.b / _datum.a * Math.Tan(he.B));
            double sphericalDistance = Math.Acos(Math.Sin(myParametricLat) * Math.Sin(hisParametricLat) + Math.Cos(myParametricLat) * Math.Cos(hisParametricLat) * Math.Cos(me.L - he.L));
            double C = Math.Pow(Math.Sin(myParametricLat) + Math.Sin(hisParametricLat), 2.0);
            double D = Math.Pow(Math.Sin(myParametricLat) - Math.Sin(hisParametricLat), 2.0);
            double P = (_datum.a - _datum.b) * (sphericalDistance - Math.Sin(sphericalDistance)) / (4 * (1 + Math.Cos(sphericalDistance)));
            double Q = (_datum.a - _datum.b) * (sphericalDistance + Math.Sin(sphericalDistance)) / (4 * (1 - Math.Cos(sphericalDistance)));
            double geodeticDistance = double.NaN;
            geodeticDistance = _datum.a * sphericalDistance - C * P - D * Q;

            return geodeticDistance;
        }
        /// <summary>
        /// XYZ座標系へ変換したオブジェクトを返す
        /// <para>参考文献</para>
        /// <para>[1]理科年表，p.563，2003．</para>
        /// <para>[2]坂井，GPSのための実用プログラミング，東京電機大学出版局，pp.28-29，2007/7．</para>
        /// <para>[3]測位航法学会　訳，精説GPS　第二版，p.126，2010/4．</para>
        /// <para>[4]杉本・柴崎，GPSハンドブック，朝倉書店，p.23 and pp.415-416，2010/9．</para>
        /// <para>[5]西修二郎訳，物理測地学，シュプリンガー・ジャパン，pp.177-179，2006/8．</para>
        /// <para>[6]西修二郎訳，GPS理論と応用，シュプリンガー・ジャパン，pp.319-320，2009/3．</para>
        /// </summary>
        /// <param name="nextDatum">変換後の測地系（2012/6/5時点では基準測地系の変換には未対応）</param>
        /// <returns>変換したXYZ座標</returns>
        public Ecef ToXYZ(Datum nextDatum)
        {
            Ecef xyz = new Ecef();
            double N;
            GlobalDatum _datum = new GlobalDatum(this.DatumKind);

            N = _datum.a / Math.Sqrt(1.0d - _datum.e2 * Math.Pow(Math.Sin(this.ToRadian().B), 2.0d));
            xyz.x = (N + this.H) * Math.Cos(this.ToRadian().B) * Math.Cos(this.ToRadian().L);
            xyz.y = (N + this.H) * Math.Cos(this.ToRadian().B) * Math.Sin(this.ToRadian().L);
            xyz.z = (N * (1.0d - _datum.e2) + this.H) * Math.Sin(this.ToRadian().B);
            // 原点や軸の向きがずれた基準座標系ならば、変換が必要
            return xyz;
        }
        /// <summary>
        /// XYZ座標系へ変換したオブジェクトを返す
        /// <para>引数を省略できます。変換後の測地系は本オブジェクトの測地系と同じになります。</para>
        /// <para>ToXYZ(Datum nextDatum)を利用して値を返します。</para>
        /// </summary>
        /// <returns>変換したXYZ座標</returns>
        public Ecef ToXYZ()
        {
            return this.ToXYZ(this.DatumKind);
        }
        /// <summary>
        /// 緯度・経度・楕円体高の文字列化
        /// Excelに合わせて、経度から先に出力する。
        /// </summary>
        /// <returns>文字列化した位置座標</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(70);
            if (this._unit == AngleUnit.Degree)
                sb.Append(this.L.ToString("0.000000")).Append(",").Append(this.B.ToString("0.000000")).Append(",").Append(this.H.ToString("0.0"));  // 有効数字が違うことに注意
            else if (this._unit == AngleUnit.Radian)
                sb.Append(this.L.ToString("0.00000000")).Append(",").Append(this.B.ToString("0.00000000")).Append(",").Append(this.H.ToString("0.0"));
            return sb.ToString();
        }
        /// <summary>
        /// 緯度と経度にフィルタをかける
        /// <para>緯度は-90～90 degの範囲とする。経度は-180～180 degの範囲とする。</para>
        /// <example>
        /// <code>
        /// double lat = 125.3, lon = -186.36;      // 極を挟んで向こう側に行って、さらに地球を半周以上している
        /// GNSS.Blh.FilterAsLatitudeAndLongitude(ref lat, ref lon, GNSS.AngleUnit.Degree);
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="lat">緯度</param>
        /// <param name="lon">経度</param>
        /// <param name="unit">単位</param>
        private static void FilterAsLatitudeAndLongitude(ref double lat, ref double lon, AngleUnit unit)
        {
            double quarter = 90.0, half = 180.0, circle = 360.0;

            if (unit == AngleUnit.Radian)
            {
                quarter = quarter * Math.PI / 180.0;
                half = half * Math.PI / 180.0;
                circle = circle * Math.PI / 180.0;
            }
            lat %= circle;                          // 地球をn周していたら、1周未満に収める
            if (lat > half)                         // 地球を半周以上していれば360°引く（又は足す）ことで±180°以内に収める
                lat -= circle;
            else if (lat < -180.0)
                lat += circle;

            if (lat > quarter || lat < -quarter)    // 極を超えているかチェック
            {
                if (lat > quarter)
                    lat = half - lat;
                else if (lat < -quarter)
                    lat = -half - lat;
                lon += half;                        // 極を挟んだ反対側に行ったわけなので、経度を180°足す
            }
            FilterAsLongitude(ref lon, unit);       // 経度に関して処理
            return;
        }
        /// <summary>
        /// 経度にフィルタをかける
        /// <para>経度は-180～180 degの範囲とする。</para>
        /// <example>
        /// <code>
        /// double lon = -186.36;                   // 地球を半周以上している
        /// GNSS.Blh.FilterAsLongitude(ref lon, GNSS.AngleUnit.Degree);
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="lon">経度</param>
        /// <param name="unit">単位</param>
        private static void FilterAsLongitude(ref double lon, AngleUnit unit)
        {
            double quarter = 90.0, half = 180.0, circle = 360.0;

            if (unit == AngleUnit.Radian)
            {
                quarter = quarter * Math.PI / 180.0;
                half = half * Math.PI / 180.0;
                circle = circle * Math.PI / 180.0;
            }
            lon %= circle;                          // 地球をn周していたら、1周未満に収める
            if (lon > half)                         // 地球を半周以上していれば360°引く（又は足す）ことで±180°以内に収める
                lon -= circle;
            else if (lon < -180.0)
                lon += circle;
            return;
        }
        /// <summary>
        /// Blhのコンストラクタ
        /// <para>引数は省略可能です。</para>
        /// </summary>
        /// <param name="B">緯度: Latitude</param>
        /// <param name="L">経度: Longitude</param>
        /// <param name="H">高度: Ellipsoidal altitude</param>
        /// <param name="unit">単位(°or rad)</param>
        /// <param name="datum">測地系<para>省略するとWGS84となる。</para></param>
        public Blh(double B = 0.0d, double L = 0.0d, double H = 0.0d, AngleUnit unit = AngleUnit.Degree, Datum datum = Datum.WGS84) 
        {
            FilterAsLatitudeAndLongitude(ref B, ref L, unit);
            this.b = B; 
            this.l = L; 
            this.h = H;
            this._unit = unit;
            this._datum = datum;
        }
        /// <summary>
        /// Blhを文字列で初期化するコンストラクタ
        /// <para>引数は省略可能です。</para>
        /// </summary>
        /// <param name="B">緯度: Latitude</param>
        /// <param name="L">経度: Longitude</param>
        /// <param name="H">高度: Ellipsoidal altitude</param>
        /// <param name="unit">単位(°or rad)</param>
        /// <param name="datum">測地系<para>省略するとWGS84となる。</para></param>
        public Blh(string B = "0.0", string L = "0.0", string H = "0.0", AngleUnit unit = AngleUnit.Degree, Datum datum = Datum.WGS84)
        {
            double b = 0.0, l = 0.0, h = 0.0;
            try
            {
                b = double.Parse(B);
                l = double.Parse(L);
                h = double.Parse(H);
                
            }
            catch(Exception)
            {
                throw new Exception("Blh構造体のコンストラクタにおいてエラーがスローされました。パラメータの文字列が解析不能です。");
            }
            FilterAsLatitudeAndLongitude(ref b, ref l, unit);
            this.b = b;
            this.l = l;
            this.h = h;
            this._unit = unit;
            this._datum = datum;
        }
    }
}
