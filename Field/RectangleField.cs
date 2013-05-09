/**************************************************************************
 * RectangleField.cs
 * 緯度経度座標系による長方形領域を指定するための構造体 for C#
 * 
 * 開発者　：森下功啓（K.Morhista Kumamoto-University）
 * 開発履歴：2012/4/24  GPS.csより独立させた。
 *           2012/5/12  全く新しいクラスになって生まれ変わった！
 *           2012/5/14  設計思想をほぼ固め、運用可能となった。
 *                      テストも一通り完了した。
 *           2012/5/15  プロパティにDifferInLat/Lonを追加
 *           2012/5/24  ToArray()を追加
 *           2012/6/4   Mapクラスへのコピーコンストラクタ実装の必要性から、本クラスについてもコピーコンストラクタを新設した。
 *           2012/8/1   領域を含むかどうかを判定するIsContain()メソッドを追加した。
 *           2012/8/2   IsContain()の実装が終わったような気がする。
 *           2012/8/10  ToString()を実装した。
 *           2012/10/1  コンストラクタに、中心座標と半径を指定するタイプのコンストラクタを新設した。
 * ***********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Geodesy.GeodeticDatum;

namespace GNSS.Field
{
    /// <summary>
    /// 緯度経度により表現する長方形領域（緯度経度座標系では長方形ですがxyz座標系では台形のような曲面となります）
    /// <para>例えば、GPSのログ領域の広さやマップの広さを表すことができる。</para>
    /// <para>経度180°をまたぐ様な領域は表現できますが、極をまたぐ領域や東西南北に対して斜めに張るような領域は表現できません。</para>
    /// <para>
    /// 2012/5/14時点では、インスタンス生成後に単位と測地系を変更することはできない。
    /// 途中での変更を可能にするには、UpperRightとUnderLeftを取得してこれをBlh構造体の機能で変換後、paramへパラメータ再格納という手順を踏む必要がある。
    /// 従ってまずはBlh構造体の拡張を進める必要がある。
    /// </para>
    /// </summary>
    public class RectangleField
    {
        /*** メンバ変数 **************************************************/
        /// <summary>
        /// パラメータ
        /// </summary>
        private FieldParameter param;
        /*** プロパティ **************************************************/
        /// <summary>
        /// 右上の座標
        /// <example>
        /// <code>
        /// RectangleField field = new RectangleField(利用可能になる初期化);
        /// Blh hoge;
        /// if (field.Available) hoge = (Blh)filed.UpperRight;
        /// </code>
        /// </example>
        /// </summary>
        public Blh UpperRight
        {
            get {
                return new Blh(this.param.upperLat, this.param.eastLon, 0, this.param.unit, this.param.datum);
            }
        }
        /// <summary>
        /// 左上の座標
        /// </summary>
        public Blh UpperLeft
        {
            get
            {
                return new Blh(this.param.upperLat, this.param.westLon, 0, this.param.unit, this.param.datum);
            }
        }
        /// <summary>
        /// 右下の座標
        /// <para>定義できない場合はnullを返します。</para>
        /// </summary>
        public Blh LowerRight
        {
            get
            {
                return new Blh(this.param.lowerLat, this.param.eastLon, 0, this.param.unit, this.param.datum);
            }
        }
        /// <summary>
        /// 左下の座標
        /// </summary>
        public Blh LowerLeft
        {
            get
            {
                return new Blh(this.param.lowerLat, this.param.westLon, 0, this.param.unit, this.param.datum);
            }
        }
        /// <summary>
        /// 中央の座標
        /// </summary>
        public Blh Center
        {
            get 
            {
                return new Blh((this.param.upperLat - this.param.lowerLat) / 2.0 + this.param.lowerLat, this.param.centerLon, 0, this.param.unit, this.param.datum);
            }
        }
        /// <summary>
        /// 面積が存在するかを返す
        /// <para>true: 面積は0。</para>
        /// </summary>
        public Boolean AreaIsZero
        {
            get {
                return this.param.AreaIsZero;
            }
        }
        /// <summary>
        /// 緯度差
        /// <para>南北方向の領域における緯度の差</para>
        /// <para>定義不可能な場合はNaNとなります。</para>
        /// <para>単位は本フィールドの定義に基づきます。単位にお気を付け下さい。</para>
        /// </summary>
        public double DifferInLat
        {
            get {
                return this.param.upperLat - this.param.lowerLat;
            }
        }
        /// <summary>
        /// 経度差
        /// <para>東西方向の領域における経度の差</para>
        /// <para>定義不可能な場合はNaNとなります。</para>
        /// <para>単位は本フィールドの定義に基づきます。単位にお気を付け下さい。</para>
        /// </summary>
        public double DifferInLon
        {
            get
            {
                Blh diff = this.UpperLeft - this.UpperRight;    // 経度の差は簡単には求まらないのでBlh構造体の機能を使って計算する
                return Math.Abs(diff.L);
            }
        }
        /// <summary>
        /// サイズ
        /// <para>領域のサイズ（緯度経度長）を返します。</para>
        /// </summary>
        public Blh Size
        {
            get 
            {
                return new Blh(this.DifferInLat, this.DifferInLon, 0, this.param.unit, this.param.datum);
            }
        }
        /*** メソッド **************************************************/
        /// <summary>
        /// 領域情報を文字列として返す
        /// <para>左上の座標と、右下の座標をカンマ区切りで返します。</para>
        /// </summary>
        /// <returns>領域情報</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(200);

            sb.Append(this.UpperLeft.ToString()).Append(",").Append(this.LowerRight.ToString());
            return sb.ToString();
        }
        /// <summary>
        /// 領域の比較を行い、本オブジェクトが引数で渡したオブジェクトを含むかどうかチェックします
        /// <para>比較は、一部の領域ではなく完全に含むかどうかを判定します。</para>
        /// </summary>
        /// <param name="comparativeField">比較対象のオブジェクト</param>
        /// <returns>true: 含む</returns>
        /// <exception cref="ArgumentException">引数のインスタンスが確保されていない場合にスロー</exception>
        public Boolean IsContain(RectangleField comparativeField)
        {
            if (comparativeField != null)
            {
                double hisLeftLon = comparativeField.LowerLeft.ToDegree().L + 180.0;    // 経度の定義域は-180 - +180であるため、比較が容易になるように180度オフセットを取る
                double hisRightLon = comparativeField.LowerRight.ToDegree().L + 180.0;
                if (hisRightLon < hisLeftLon) hisRightLon += 360.0;                     // 経度180度をまたがっている場合の対策
                double hisUpperLat = comparativeField.UpperLeft.ToDegree().B;
                double hisLowerLat = comparativeField.LowerLeft.ToDegree().B;
                double myLeftLon = this.LowerLeft.ToDegree().L + 180.0;
                double myRightLon = this.LowerRight.ToDegree().L + 180.0;
                if (myRightLon < myLeftLon)                                             // 経度180度をまたがっていた場合の対策
                {
                    if (hisLeftLon < myRightLon)                                        // 見切れている範囲内に比較対象の領域がかかっていた場合
                    {
                        hisRightLon += 360.0;                                           // 1サイクル分進める
                        hisLeftLon += 360.0;
                    }
                    myRightLon += 360.0;                                                // こちらも1サイクル進める
                }
                double myUpperLat = this.UpperLeft.ToDegree().B;
                double myLowerLat = this.LowerLeft.ToDegree().B;
                if (hisLeftLon >= myLeftLon && hisRightLon <= myRightLon &&             // 自身の中に、比較対象の領域が完全に含まれているか確認する
                    hisUpperLat <= myUpperLat && hisLowerLat >= myLowerLat)
                    return true;
                else
                    return false;
            }
            else
                throw new ArgumentException("RectangleFieldクラスのIsContain()にてエラーがスローされました。引数のインスタンスは確保してください。");
        }
        /// <summary>
        /// 領域を指定マージン[km]にて拡張した領域を返します
        /// <para>非破壊的メソッド</para>
        /// <para>経度方向は高緯度に合わせ、緯度方向は低緯度に合わせて拡張します。</para>
        /// </summary>
        /// <param name="marginKm">マージン[km]<para>負値の場合は処理を行いません。</para></param>
        /// <returns>拡張済みのオブジェクト<para>引数のmarginが負値の場合はnullを返します。</para></returns>
        public RectangleField Extend(double marginKm)
        {
            RectangleField ans = null;
            if (marginKm > 0)
            {
                ans = new RectangleField(this);
                Blh highLat, lowLat;
                if (Math.Abs(this.LowerLeft.B) > Math.Abs(this.UpperLeft.B))
                {
                    highLat = this.LowerLeft;
                    lowLat = this.UpperLeft;
                }
                else
                {
                    highLat = this.UpperLeft;
                    lowLat = this.LowerLeft;
                }
                double n = marginKm * 1000.0 / lowLat.GetUnitLengthForEN().N;      // 緯度が低い方が緯度方向の単位距離が短い
                double e = marginKm * 1000.0 / highLat.GetUnitLengthForEN().E;     // 緯度が高い方が経度方向の単位距離は短い
                Blh pad = new Blh(n, e);
                ans.Set(this.UpperRight + pad);
                ans.Set(this.LowerLeft - pad);
            }
            return ans;
        }
        /// <summary>
        /// 領域をBlh型の配列として返す
        /// <para>格納の順番はKMLのポリゴンで四角く描かれる順番にしています。</para>
        /// </summary>
        /// <returns>領域の四隅をBlh型の配列に加工したもの</returns>
        public Blh[] ToArray()
        {
            Blh[] arr = new Blh[4];
            arr[0] = this.UpperLeft;
            arr[1] = this.UpperRight;
            arr[2] = this.LowerRight;
            arr[3] = this.LowerLeft;
            return arr;
        }
        /// <summary>
        /// 緯度経度を更新する
        /// </summary>
        /// <param name="additionalPos">追加座標</param>
        /// <param name="param">領域パラメータ</param>
        private static void SetLatAndEastWestLon(Blh additionalPos, ref FieldParameter param)
        {
            additionalPos.Unit = param.unit;                                    // 単位を変換（統一する）
            additionalPos.DatumKind = param.datum;                              // 測地系を統一
            // まずは緯度の値をセット（これは単純で良い）
            if (double.IsNaN(param.upperLat)) param.upperLat = additionalPos.B; // 未セットならセットするし、保持データと比べて大きい/小さければ格納する。
            if (param.upperLat < additionalPos.B) param.upperLat = additionalPos.B;
            if (double.IsNaN(param.lowerLat)) param.lowerLat = additionalPos.B;
            if (param.lowerLat > additionalPos.B) param.lowerLat = additionalPos.B;
            // 次に経度の処理
            if (double.IsNaN(param.eastLon)) param.eastLon = additionalPos.L;   // 未セットならセット
            if (double.IsNaN(param.westLon)) param.westLon = additionalPos.L;
            if (double.IsNaN(param.centerLon))
            {
                param.centerLon = additionalPos.L;                                // 初回時は代入するだけ
            }
            else
            {
                // まずは経度の東端と西端の処理
                Blh currentCenter = new Blh(0, param.centerLon, 0, param.unit, param.datum);        // 中央の経度を持つ座標を生成（このメソッド中では、緯度は0で宣言して引き算しないと、Blh構造体の仕様による要因で計算を誤る。）
                Blh relativeNewPos = additionalPos - currentCenter;                                 // currentCenter基準の相対座標を計算
                if (relativeNewPos.L > 0.0)                                                         // additionalPosがcurrentCenterよりも東側にある
                {
                    Blh currentEast = new Blh(0, param.eastLon, 0, param.unit, param.datum);        // 東端の経度を持つ座標を生成
                    Blh relativeCurrentEast = currentEast - currentCenter;                          // currentCenter基準の現東端の相対座標を計算
                    if (relativeNewPos.L > relativeCurrentEast.L) param.eastLon = additionalPos.L;  // より東に属する方を採用する
                }
                else if (relativeNewPos.L < 0.0)                                                    // additionalPosがcurrentCenterよりも西側にある
                {
                    Blh currentWest = new Blh(0, param.westLon, 0, param.unit, param.datum);        // 西端の経度を持つ座標を生成
                    Blh relativeCurrentWest = currentWest - currentCenter;                          // currentCenter基準の現西端の相対座標を計算
                    if (relativeNewPos.L < relativeCurrentWest.L) param.westLon = additionalPos.L;  // より西に属する方を採用する
                }
                // 次に経度中央の計算（変数のスコープがうざいのでかっこを付けてスコープを限定化する）
                // 以下は新規追加の座標が中央座標に対してπ以上の更新量が無ければ成り立つ。上のアルゴリズムを変更する際には細心の注意を払うこと。
                {
                    Blh currentWest = new Blh(0, param.westLon, 0, param.unit, param.datum);        // 西端の経度を持つ座標を生成
                    Blh currentEast = new Blh(0, param.eastLon, 0, param.unit, param.datum);        // 東端の経度を持つ座標を生成
                    Blh candidate = currentWest.GetMedian(currentEast);                             // 中間の座標を計算する。この時点では領域のセンターかどうかは不明。
                    Blh relationCandidate = candidate - currentEast;                                // 東端を基準に見た候補座標の相対座標を計算
                    Blh relatinCurrentCenter = currentCenter - currentEast;                         // 東端を基準に見た現中央座標の相対座標を計算
                    double half = 180.0;
                    if (param.unit == AngleUnit.Radian) half = half * Math.PI / 180.0;              // 経度をπだけ進める準備
                    if (relationCandidate.L * relatinCurrentCenter.L < 0) candidate = new Blh(0, candidate.L + half, 0, param.unit, param.datum); // 地球の反対側でなかったかをチェックして、反対側ならπ進める
                    param.centerLon = candidate.L;                                                  // 中心座標を更新
                }
            }
            return;
        }
        /// <summary>
        /// 座標をセットする
        /// <para>自動的に過去の履歴と比較されて領域が拡張されます。</para>
        /// </summary>
        /// <param name="pos">セットする座標</param>
        public void Set(Blh pos)
        {
            SetLatAndEastWestLon(pos, ref this.param);
            return;
        }
        /// <summary>
        /// 座標の配列から領域を生成する
        /// <para>自動的に過去の履歴と比較されて領域が拡張されます。</para>
        /// </summary>
        /// <param name="pos">セットする座標</param>
        public void Set(Blh[] pos)
        {
            foreach (Blh _pos in pos) this.Set(_pos);
        }
        /// <summary>
        /// コンストラクタ
        /// <para>中心座標と半径[km]を指定すると、その円を格納できる四角い領域を宣言します。</para>
        /// </summary>
        /// <param name="x">中心座標</param>
        /// <param name="radius">半径[km]</param>
        public RectangleField(Blh x, double radius)
        {
            this.param = new FieldParameter(x.Unit, x.DatumKind);
            var len = x.GetUnitLengthForEN();
            var dn = radius * 1000.0 / len.N;
            var de = radius * 1000.0 / len.E;
            var pos = new Blh(dn, de, 0.0, AngleUnit.Degree, x.DatumKind);
            x.Unit = AngleUnit.Degree;
            RectangleField.SetLatAndEastWestLon(x - pos, ref this.param);
            RectangleField.SetLatAndEastWestLon(x + pos, ref this.param);
        }
        /// <summary>
        /// コンストラクタ
        /// <para>2つの座標を指定すると、大小関係からマップの座標を自動的に決定する。</para>
        /// <para>経度に関しては、より小さな領域となる座標を選択する。</para>
        /// </summary>
        /// <param name="x1">座標1</param>
        /// <param name="x2">座標2</param>
        /// <param name="unit">単位(°or rad)</param>
        /// <param name="datum">測地系</param>
        public RectangleField(Blh x1, Blh x2, AngleUnit unit = AngleUnit.Degree, Datum datum = Datum.WGS84)
        {
            this.param = new FieldParameter(unit, datum);
            RectangleField.SetLatAndEastWestLon(x1, ref this.param);
            RectangleField.SetLatAndEastWestLon(x2, ref this.param);
        }
        /// <summary>
        /// 座標省略時のコンストラクタ
        /// <para>内部パラメータには非値が代入され、使用準備は完了しますが値をセットしない限り運用できません。</para>
        /// </summary>
        /// <param name="unit">単位(°or rad)</param>
        /// <param name="datum">測地系</param>
        public RectangleField(AngleUnit unit = AngleUnit.Degree, Datum datum = Datum.WGS84)
        {
            this.param = new FieldParameter(unit, datum);
        }
        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="field">コピー元のインスタンス</param>
        public RectangleField(RectangleField field)
        {
            this.param = field.param;
        }
    }
}
