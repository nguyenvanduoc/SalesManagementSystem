using System;

namespace SalesManagementSystem.Helpers
{
    public static class NumberToTextHelper
    {
        public static string DocTienBangChu(decimal total)
        {
            try
            {
                string rs = "";
                total = Math.Round(total, 0);
                if (total == 0) return "Không đồng";
                string[] ch = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
                string[] rch = { "lẻ", "mốt", "", "", "", "lăm" };
                string[] u = { "", "mươi", "trăm", "ngàn", "", "", "triệu", "", "", "tỷ", "", "", "ngàn", "", "", "triệu" };
                string nstr = total.ToString();

                int[] n = new int[nstr.Length];
                int len = n.Length;
                for (int i = 0; i < len; i++)
                {
                    n[len - 1 - i] = Convert.ToInt32(nstr.Substring(i, 1));
                }

                for (int i = len - 1; i >= 0; i--)
                {
                    if (i % 3 == 2)// số 0 ở hàng trăm
                    {
                        if (n[i] == 0 && n[i - 1] == 0 && n[i - 2] == 0) continue;//nếu cả 3 số là 0 thì bỏ qua không đọc
                    }
                    else if (i % 3 == 1) // số ở hàng chục
                    {
                        if (n[i] == 0)
                        {
                            if (n[i - 1] == 0) { continue; }// nếu hàng chục và hàng đơn vị đều là 0 thì bỏ qua.
                            else
                            {
                                rs += " " + rch[0]; continue;// hàng chục là 0 thì đọc là lẻ
                            }
                        }
                        if (n[i] == 1)//nếu số hàng chục là 1 thì đọc là mười
                        {
                            rs += " mười"; continue;
                        }
                    }
                    else if (i != len - 1)// số ở hàng đơn vị (không phải là số đầu tiên)
                    {
                        if (n[i] == 0)// số hàng đơn vị là 0 thì chỉ đọc đơn vị
                        {
                            if (i + 2 <= len - 1 && n[i + 2] == 0 && n[i + 1] == 0) continue;
                            rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);
                            continue;
                        }
                        if (n[i] == 1)// nếu là 1 thì tùy vào số hàng chục mà đọc: 0,1: một / còn lại: mốt
                        {
                            int ten = (i + 1 <= len - 1) ? n[i + 1] : 0;
                            if (ten == 0 || ten == 1)
                                rs += " " + ch[n[i]];
                            else
                                rs += " " + rch[1];
                            rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);
                            continue;
                        }
                        if (n[i] == 5) // cách đọc số 5
                        {
                            if (i + 1 <= len - 1 && n[i + 1] != 0) //nếu số hàng chục khác 0 thì đọc số 5 là lăm
                                rs += " " + rch[5];
                            else
                                rs += " " + ch[n[i]];// đọc số 5 là năm
                            rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);
                            continue;
                        }
                    }
                    rs += (rs == "" ? " " : ", ") + ch[n[i]];// đọc số
                    rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);// đọc đơn vị
                }
                if (rs.Length > 0 && rs.Substring(0, 1) == ",") rs = rs.Substring(2);
                rs = rs.Trim();
                rs = rs.Replace("lẻ,", "lẻ");
                rs = rs.Replace("mươi,", "mươi");
                rs = rs.Replace("trăm,", "trăm");
                rs = rs.Replace("mười,", "mười");
                if (rs.Length > 0)
                {
                    rs = rs.Substring(0, 1).ToUpper() + rs.Substring(1);
                }
                return rs + " đồng";
            }
            catch
            {
                return "";
            }
        }
    }
}
