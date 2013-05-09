/*************************************
 * FieldKind.cs
 * 領域の表し方を規定する列挙体 for C#
 * 
 * 開発者　：森下功啓（K.Morhista Kumamoto-University）
 * 開発履歴：
 *          2012/5/11   新設。作りはしたけど、拡張するかどうかは不明。
 * 
 * *****************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GNSS.Field
{
    /// <summary>
    /// 領域の表し方を規定する列挙体
    /// </summary>
    public enum FieldKind
    {
        /// <summary>
        /// 長方形の領域
        /// <para>高さ方向に厚みがなくてもエラー扱いとはしません。</para>
        /// </summary>
        Rectangle,
        /// <summary>
        /// 円形領域
        /// <para>高さ方向に厚みがなくてもエラー扱いとはしません。</para>
        /// </summary>
        Circlle,
        /// <summary>
        /// 直方体
        /// </summary>
        RectangularParallelepiped,
        /// <summary>
        /// 球状領域
        /// </summary>
        Shere
    }
}
