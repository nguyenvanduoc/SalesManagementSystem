using System.Collections.Generic;

namespace SalesManagementSystem.Models.ViewModels
{
    public class PhanQuyenMatrixVM
    {
        public string NhomChaManHinh { get; set; }
        public List<PhanQuyenScreenVM> Screens { get; set; }

        public PhanQuyenMatrixVM()
        {
            Screens = new List<PhanQuyenScreenVM>();
        }
    }

    public class PhanQuyenScreenVM
    {
        public int IDManHinh { get; set; }
        public string TenManHinh { get; set; }
        public List<PhanQuyenActionVM> Actions { get; set; }

        public PhanQuyenScreenVM()
        {
            Actions = new List<PhanQuyenActionVM>();
        }
    }

    public class PhanQuyenActionVM
    {
        public int IDAction { get; set; }
        public int LoaiPhanQuyen { get; set; }
        public string GhiChu { get; set; }
        public bool IsChoPhep { get; set; }
    }
}
