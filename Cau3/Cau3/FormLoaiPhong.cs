using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Cau3_App.Models;

namespace Cau3_App
{
    public partial class FormLoaiPhong : Form
    {
        private SunriseHomestayContext db = new SunriseHomestayContext();
        
        private TextBox txtTenLoai = new TextBox();
        private TextBox txtGiaMoiDem = new TextBox();
        private TextBox txtMoTa = new TextBox();
        private Button btnThem = new Button();
        private Button btnSua = new Button();
        private Button btnXoa = new Button();
        private Button btnLamMoi = new Button();
        private DataGridView dgvLoaiPhong = new DataGridView();
        private int selectedId = -1;

        public FormLoaiPhong()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý Loại phòng";
            this.Size = new System.Drawing.Size(650, 430);
            this.StartPosition = FormStartPosition.CenterParent;

            Label lbl1 = new Label() { Text = "Tên loại:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            txtTenLoai.Location = new System.Drawing.Point(100, 18);
            txtTenLoai.Width = 140;
            
            Label lbl2 = new Label() { Text = "Giá/đêm:", Location = new System.Drawing.Point(20, 60), AutoSize = true };
            txtGiaMoiDem.Location = new System.Drawing.Point(100, 58);
            txtGiaMoiDem.Width = 140;

            Label lbl3 = new Label() { Text = "Mô tả:", Location = new System.Drawing.Point(260, 20), AutoSize = true };
            txtMoTa.Location = new System.Drawing.Point(310, 18);
            txtMoTa.Multiline = true;
            txtMoTa.Size = new System.Drawing.Size(290, 60);

            btnThem.Text = "Thêm"; btnThem.Location = new System.Drawing.Point(20, 100); btnThem.Size = new System.Drawing.Size(75, 30);
            btnThem.Click += BtnThem_Click;

            btnSua.Text = "Sửa"; btnSua.Location = new System.Drawing.Point(105, 100); btnSua.Size = new System.Drawing.Size(75, 30);
            btnSua.Click += BtnSua_Click;

            btnXoa.Text = "Xóa"; btnXoa.Location = new System.Drawing.Point(190, 100); btnXoa.Size = new System.Drawing.Size(75, 30);
            btnXoa.Click += BtnXoa_Click;

            btnLamMoi.Text = "Làm mới"; btnLamMoi.Location = new System.Drawing.Point(275, 100); btnLamMoi.Size = new System.Drawing.Size(75, 30);
            btnLamMoi.Click += (s, e) => ResetFields();

            dgvLoaiPhong.Location = new System.Drawing.Point(20, 145);
            dgvLoaiPhong.Size = new System.Drawing.Size(580, 220);
            dgvLoaiPhong.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLoaiPhong.ReadOnly = true;
            dgvLoaiPhong.AllowUserToAddRows = false;
            dgvLoaiPhong.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLoaiPhong.CellClick += DgvLoaiPhong_CellClick;

            this.Controls.Add(lbl1); this.Controls.Add(txtTenLoai);
            this.Controls.Add(lbl2); this.Controls.Add(txtGiaMoiDem);
            this.Controls.Add(lbl3); this.Controls.Add(txtMoTa);
            this.Controls.Add(btnThem); this.Controls.Add(btnSua);
            this.Controls.Add(btnXoa); this.Controls.Add(btnLamMoi);
            this.Controls.Add(dgvLoaiPhong);

            this.Load += FormLoaiPhong_Load;
            this.FormClosed += (s, e) => db.Dispose();
        }

        private void FormLoaiPhong_Load(object? sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                dgvLoaiPhong.DataSource = db.LoaiPhongs.AsNoTracking().Select(x => new {
                    x.MaLoai,
                    x.TenLoai,
                    x.GiaMoiDem,
                    x.MoTa
                }).ToList();

                if (dgvLoaiPhong.Columns["MaLoai"] is DataGridViewColumn colMaLoai) colMaLoai.HeaderText = "Mã loại";
                if (dgvLoaiPhong.Columns["TenLoai"] is DataGridViewColumn colTenLoai) colTenLoai.HeaderText = "Tên loại";
                if (dgvLoaiPhong.Columns["GiaMoiDem"] is DataGridViewColumn colGia)
                {
                    colGia.HeaderText = "Giá/đêm (VNĐ)";
                    colGia.DefaultCellStyle.Format = "N0";
                }
                if (dgvLoaiPhong.Columns["MoTa"] is DataGridViewColumn colMoTa) colMoTa.HeaderText = "Mô tả";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách loại phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetFields()
        {
            txtTenLoai.Text = "";
            txtGiaMoiDem.Text = "";
            txtMoTa.Text = "";
            selectedId = -1;
        }

        private void DgvLoaiPhong_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvLoaiPhong.Rows.Count)
            {
                var row = dgvLoaiPhong.Rows[e.RowIndex];
                if (row.Cells["MaLoai"].Value != null)
                {
                    selectedId = Convert.ToInt32(row.Cells["MaLoai"].Value);
                    txtTenLoai.Text = row.Cells["TenLoai"].Value?.ToString() ?? "";
                    txtGiaMoiDem.Text = row.Cells["GiaMoiDem"].Value?.ToString() ?? "";
                    txtMoTa.Text = row.Cells["MoTa"].Value?.ToString() ?? "";
                }
            }
        }

        private void BtnThem_Click(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTenLoai.Text))
                {
                    MessageBox.Show("Vui lòng nhập tên loại phòng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTenLoai.Focus();
                    return;
                }

                decimal gia = 0;
                if (!string.IsNullOrWhiteSpace(txtGiaMoiDem.Text) && !decimal.TryParse(txtGiaMoiDem.Text.Replace(",", "").Replace(".", ""), out gia))
                {
                    MessageBox.Show("Giá mỗi đêm không hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtGiaMoiDem.Focus();
                    return;
                }

                var lp = new LoaiPhong
                {
                    TenLoai = txtTenLoai.Text.Trim(),
                    GiaMoiDem = gia,
                    MoTa = txtMoTa.Text.Trim()
                };
                db.LoaiPhongs.Add(lp);
                db.SaveChanges();
                MessageBox.Show("Thêm loại phòng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
                ResetFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm loại phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            try
            {
                if (selectedId == -1)
                {
                    MessageBox.Show("Vui lòng chọn loại phòng cần sửa từ bảng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtTenLoai.Text))
                {
                    MessageBox.Show("Vui lòng nhập tên loại phòng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTenLoai.Focus();
                    return;
                }

                decimal gia = 0;
                if (!string.IsNullOrWhiteSpace(txtGiaMoiDem.Text) && !decimal.TryParse(txtGiaMoiDem.Text.Replace(",", "").Replace(".", ""), out gia))
                {
                    MessageBox.Show("Giá mỗi đêm không hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtGiaMoiDem.Focus();
                    return;
                }

                var lp = db.LoaiPhongs.Find(selectedId);
                if (lp != null)
                {
                    lp.TenLoai = txtTenLoai.Text.Trim();
                    lp.GiaMoiDem = gia;
                    lp.MoTa = txtMoTa.Text.Trim();
                    db.SaveChanges();
                    MessageBox.Show("Cập nhật loại phòng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    ResetFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi sửa loại phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            try
            {
                if (selectedId == -1)
                {
                    MessageBox.Show("Vui lòng chọn loại phòng cần xóa từ bảng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Kiểm tra ràng buộc khóa ngoại trước khi xóa
                bool dangCoPhong = db.Phongs.Any(p => p.MaLoai == selectedId);
                if (dangCoPhong)
                {
                    MessageBox.Show("Không thể xóa loại phòng này vì đang có phòng thuộc loại phòng này!", "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show("Bạn có chắc chắn muốn xóa loại phòng này?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var lp = db.LoaiPhongs.Find(selectedId);
                    if (lp != null)
                    {
                        db.LoaiPhongs.Remove(lp);
                        db.SaveChanges();
                        MessageBox.Show("Xóa loại phòng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                        ResetFields();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa loại phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
