using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Cau3_App.Models;

namespace Cau3_App
{
    public partial class Form1 : Form
    {
        private SunriseHomestayContext db;
        
        private TextBox txtSoPhong;
        private NumericUpDown nudTangSo;
        private ComboBox cboLoaiPhong;
        private ComboBox cboTinhTrang;
        private PictureBox picHinhAnh;
        private Button btnChonAnh;
        private Button btnThem, btnSua, btnXoa, btnLamMoi;
        private ComboBox cboLocLoaiPhong, cboLocTinhTrang;
        private Button btnTimKiem;
        private DataGridView dgvPhong;
        
        private string hinhAnhFileName = "";
        private int selectedPhongId = -1;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản Lý Phòng - Sunrise Homestay";
            this.Size = new Size(1000, 620);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Labels and Inputs (startY = 35 để không bị MenuStrip đè lên)
            int startX = 20;
            int startY = 35;

            Label lblSoPhong = new Label() { Text = "Số phòng", Location = new Point(startX, startY), AutoSize = true };
            txtSoPhong = new TextBox() { Location = new Point(startX, startY + 20), Width = 100 };

            Label lblTangSo = new Label() { Text = "Tầng số", Location = new Point(startX + 120, startY), AutoSize = true };
            nudTangSo = new NumericUpDown() { Location = new Point(startX + 120, startY + 20), Width = 80, Minimum = 1, Maximum = 100 };

            Label lblLoaiPhong = new Label() { Text = "Loại phòng", Location = new Point(startX + 220, startY), AutoSize = true };
            cboLoaiPhong = new ComboBox() { Location = new Point(startX + 220, startY + 20), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblTinhTrang = new Label() { Text = "Tình trạng", Location = new Point(startX + 360, startY), AutoSize = true };
            cboTinhTrang = new ComboBox() { Location = new Point(startX + 360, startY + 20), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cboTinhTrang.Items.AddRange(new string[] { "Trống", "Đang ở", "Đang dọn" });
            cboTinhTrang.SelectedIndex = 0;

            picHinhAnh = new PictureBox() { Location = new Point(startX + 500, startY), Size = new Size(100, 80), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.StretchImage };
            btnChonAnh = new Button() { Text = "Chọn ảnh...", Location = new Point(startX + 500, startY + 85), Width = 100 };
            btnChonAnh.Click += BtnChonAnh_Click;

            // Buttons
            btnThem = new Button() { Text = "Thêm", Location = new Point(startX + 650, startY + 20), Size = new Size(75, 30) };
            btnThem.Click += BtnThem_Click;
            btnSua = new Button() { Text = "Sửa", Location = new Point(startX + 735, startY + 20), Size = new Size(75, 30) };
            btnSua.Click += BtnSua_Click;
            btnXoa = new Button() { Text = "Xóa", Location = new Point(startX + 820, startY + 20), Size = new Size(75, 30) };
            btnXoa.Click += BtnXoa_Click;
            btnLamMoi = new Button() { Text = "Làm mới", Location = new Point(startX + 905, startY + 20), Size = new Size(75, 30) };
            btnLamMoi.Click += (s, e) => BtnLamMoi_Click(s, e);

            // Filter
            int filterY = startY + 120;
            cboLocLoaiPhong = new ComboBox() { Location = new Point(startX, filterY), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cboLocTinhTrang = new ComboBox() { Location = new Point(startX + 170, filterY), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cboLocTinhTrang.Items.AddRange(new string[] { "Tất cả", "Trống", "Đang ở", "Đang dọn" });
            cboLocTinhTrang.SelectedIndex = 0;
            btnTimKiem = new Button() { Text = "Tìm kiếm", Location = new Point(startX + 340, filterY - 2), Size = new Size(80, 26) };
            btnTimKiem.Click += BtnTimKiem_Click;

            // DataGridView
            dgvPhong = new DataGridView() { Location = new Point(startX, filterY + 30), Size = new Size(950, 360), ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, RowTemplate = { Height = 60 } };
            dgvPhong.CellClick += DgvPhong_CellClick;

            this.Controls.Add(lblSoPhong); this.Controls.Add(txtSoPhong);
            this.Controls.Add(lblTangSo); this.Controls.Add(nudTangSo);
            this.Controls.Add(lblLoaiPhong); this.Controls.Add(cboLoaiPhong);
            this.Controls.Add(lblTinhTrang); this.Controls.Add(cboTinhTrang);
            this.Controls.Add(picHinhAnh); this.Controls.Add(btnChonAnh);
            this.Controls.Add(btnThem); this.Controls.Add(btnSua);
            this.Controls.Add(btnXoa); this.Controls.Add(btnLamMoi);
            this.Controls.Add(cboLocLoaiPhong); this.Controls.Add(cboLocTinhTrang);
            this.Controls.Add(btnTimKiem);
            this.Controls.Add(dgvPhong);

            // MenuStrip
            MenuStrip menu = new MenuStrip();
            ToolStripMenuItem mnuQuanLy = new ToolStripMenuItem("Hệ thống");
            ToolStripMenuItem mnuLoaiPhong = new ToolStripMenuItem("Quản lý Loại phòng");
            mnuLoaiPhong.Click += (s, e) => {
                using (var form = new FormLoaiPhong())
                {
                    form.ShowDialog();
                }
                LoadLoaiPhong();
                LoadData();
            };
            mnuQuanLy.DropDownItems.Add(mnuLoaiPhong);
            menu.Items.Add(mnuQuanLy);
            this.MainMenuStrip = menu;
            this.Controls.Add(menu);

            this.Load += Form1_Load;
        }

        private Image? LoadImageSafe(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return null;
                byte[] bytes = File.ReadAllBytes(filePath);
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    return Image.FromStream(ms);
                }
            }
            catch
            {
                return null;
            }
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            try
            {
                db = new SunriseHomestayContext();
                string imgDir = Path.Combine(Application.StartupPath, "Images");
                if (!Directory.Exists(imgDir)) Directory.CreateDirectory(imgDir);

                LoadLoaiPhong();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối cơ sở dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadLoaiPhong()
        {
            try
            {
                var dsLoaiPhong = db.LoaiPhongs.AsNoTracking().ToList();
                
                cboLoaiPhong.DataSource = dsLoaiPhong;
                cboLoaiPhong.DisplayMember = "TenLoai";
                cboLoaiPhong.ValueMember = "MaLoai";
                if (dsLoaiPhong.Count > 0) cboLoaiPhong.SelectedIndex = 0;

                var dsLoc = dsLoaiPhong.ToList();
                dsLoc.Insert(0, new LoaiPhong { MaLoai = 0, TenLoai = "Tất cả" });
                cboLocLoaiPhong.DataSource = dsLoc;
                cboLocLoaiPhong.DisplayMember = "TenLoai";
                cboLocLoaiPhong.ValueMember = "MaLoai";
                cboLocLoaiPhong.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải loại phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadData(int filterLoaiPhong = 0, string filterTinhTrang = "Tất cả")
        {
            try
            {
                var query = db.Phongs.AsNoTracking().Include(x => x.MaLoaiNavigation).AsQueryable();
                if (filterLoaiPhong != 0)
                    query = query.Where(x => x.MaLoai == filterLoaiPhong);
                if (!string.IsNullOrEmpty(filterTinhTrang) && filterTinhTrang != "Tất cả")
                    query = query.Where(x => x.TinhTrang == filterTinhTrang);

                var list = query.ToList();

                dgvPhong.Columns.Clear();
                dgvPhong.AutoGenerateColumns = false;

                dgvPhong.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaPhong", DataPropertyName = "MaPhong", HeaderText = "Mã phòng", Width = 90 });
                
                DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
                imgCol.Name = "Anh";
                imgCol.HeaderText = "Ảnh";
                imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
                imgCol.Width = 80;
                dgvPhong.Columns.Add(imgCol);

                dgvPhong.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoPhong", DataPropertyName = "SoPhong", HeaderText = "Số phòng", Width = 110 });
                dgvPhong.Columns.Add(new DataGridViewTextBoxColumn { Name = "TangSo", DataPropertyName = "TangSo", HeaderText = "Tầng", Width = 70 });
                dgvPhong.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenLoai", HeaderText = "Loại phòng", Width = 140 });
                dgvPhong.Columns.Add(new DataGridViewTextBoxColumn { Name = "GiaMoiDem", HeaderText = "Giá/đêm (VNĐ)", Width = 130 });
                dgvPhong.Columns.Add(new DataGridViewTextBoxColumn { Name = "TinhTrang", DataPropertyName = "TinhTrang", HeaderText = "Tình trạng", Width = 120 });
                dgvPhong.Columns.Add(new DataGridViewTextBoxColumn { Name = "HinhAnh", DataPropertyName = "HinhAnh", Visible = false });
                dgvPhong.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaLoai", DataPropertyName = "MaLoai", Visible = false });

                dgvPhong.DataSource = list;

                foreach (DataGridViewRow row in dgvPhong.Rows)
                {
                    if (row.DataBoundItem is Phong p)
                    {
                        row.Cells["TenLoai"].Value = p.MaLoaiNavigation?.TenLoai;
                        row.Cells["GiaMoiDem"].Value = p.MaLoaiNavigation?.GiaMoiDem?.ToString("N0");

                        if (!string.IsNullOrEmpty(p.HinhAnh))
                        {
                            string path = Path.Combine(Application.StartupPath, "Images", p.HinhAnh);
                            row.Cells["Anh"].Value = LoadImageSafe(path);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnChonAnh_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string destFolder = Path.Combine(Application.StartupPath, "Images");
                        if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);

                        string fileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + Path.GetFileName(ofd.FileName);
                        string destFile = Path.Combine(destFolder, fileName);
                        
                        File.Copy(ofd.FileName, destFile, true);

                        hinhAnhFileName = fileName;
                        picHinhAnh.Image?.Dispose();
                        picHinhAnh.Image = LoadImageSafe(destFile);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi chọn ảnh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DgvPhong_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvPhong.Rows.Count)
            {
                var row = dgvPhong.Rows[e.RowIndex];
                if (row.Cells["MaPhong"].Value != null)
                {
                    selectedPhongId = Convert.ToInt32(row.Cells["MaPhong"].Value);
                    txtSoPhong.Text = row.Cells["SoPhong"].Value?.ToString() ?? "";
                    
                    decimal tangVal = 1;
                    if (row.Cells["TangSo"].Value != null && decimal.TryParse(row.Cells["TangSo"].Value?.ToString(), out decimal parsed))
                    {
                        tangVal = parsed;
                    }
                    nudTangSo.Value = Math.Max(nudTangSo.Minimum, Math.Min(nudTangSo.Maximum, tangVal <= 0 ? 1 : tangVal));

                    if (row.Cells["MaLoai"].Value != null)
                    {
                        cboLoaiPhong.SelectedValue = row.Cells["MaLoai"].Value;
                    }

                    if (row.Cells["TinhTrang"].Value != null)
                    {
                        cboTinhTrang.SelectedItem = row.Cells["TinhTrang"].Value?.ToString();
                    }

                    hinhAnhFileName = row.Cells["HinhAnh"].Value?.ToString() ?? "";
                    
                    picHinhAnh.Image?.Dispose();
                    picHinhAnh.Image = null;
                    if (!string.IsNullOrEmpty(hinhAnhFileName))
                    {
                        string path = Path.Combine(Application.StartupPath, "Images", hinhAnhFileName);
                        picHinhAnh.Image = LoadImageSafe(path);
                    }
                }
            }
        }

        private void BtnLamMoi_Click(object? sender, EventArgs? e)
        {
            txtSoPhong.Text = "";
            nudTangSo.Value = 1;
            if (cboLoaiPhong.Items.Count > 0) cboLoaiPhong.SelectedIndex = 0;
            if (cboTinhTrang.Items.Count > 0) cboTinhTrang.SelectedIndex = 0;
            picHinhAnh.Image?.Dispose();
            picHinhAnh.Image = null;
            hinhAnhFileName = "";
            selectedPhongId = -1;
            if (cboLocLoaiPhong.Items.Count > 0) cboLocLoaiPhong.SelectedIndex = 0;
            if (cboLocTinhTrang.Items.Count > 0) cboLocTinhTrang.SelectedIndex = 0;
            LoadData();
        }

        private void BtnThem_Click(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtSoPhong.Text))
                {
                    MessageBox.Show("Vui lòng nhập số phòng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSoPhong.Focus();
                    return;
                }

                if (cboLoaiPhong.SelectedValue == null || !int.TryParse(cboLoaiPhong.SelectedValue.ToString(), out int maLoai) || maLoai <= 0)
                {
                    MessageBox.Show("Vui lòng chọn loại phòng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string tinhTrang = cboTinhTrang.SelectedItem?.ToString() ?? "Trống";

                var p = new Phong
                {
                    SoPhong = txtSoPhong.Text.Trim(),
                    TangSo = (int)nudTangSo.Value,
                    MaLoai = maLoai,
                    TinhTrang = tinhTrang,
                    HinhAnh = hinhAnhFileName
                };
                db.Phongs.Add(p);
                db.SaveChanges();
                MessageBox.Show("Thêm phòng mới thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                BtnLamMoi_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            try
            {
                if (selectedPhongId == -1)
                {
                    MessageBox.Show("Vui lòng chọn phòng cần sửa từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtSoPhong.Text))
                {
                    MessageBox.Show("Vui lòng nhập số phòng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSoPhong.Focus();
                    return;
                }

                if (cboLoaiPhong.SelectedValue == null || !int.TryParse(cboLoaiPhong.SelectedValue.ToString(), out int maLoai) || maLoai <= 0)
                {
                    MessageBox.Show("Vui lòng chọn loại phòng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var p = db.Phongs.Find(selectedPhongId);
                if (p != null)
                {
                    p.SoPhong = txtSoPhong.Text.Trim();
                    p.TangSo = (int)nudTangSo.Value;
                    p.MaLoai = maLoai;
                    p.TinhTrang = cboTinhTrang.SelectedItem?.ToString() ?? "Trống";
                    p.HinhAnh = hinhAnhFileName;
                    db.SaveChanges();
                    MessageBox.Show("Cập nhật thông tin phòng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    BtnLamMoi_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi sửa phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            try
            {
                if (selectedPhongId == -1)
                {
                    MessageBox.Show("Vui lòng chọn phòng cần xóa từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show("Bạn có chắc chắn muốn xóa phòng này?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var p = db.Phongs.Find(selectedPhongId);
                    if (p != null)
                    {
                        db.Phongs.Remove(p);
                        db.SaveChanges();
                        MessageBox.Show("Xóa phòng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        BtnLamMoi_Click(null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnTimKiem_Click(object? sender, EventArgs e)
        {
            int loai = 0;
            if (cboLocLoaiPhong.SelectedValue != null && int.TryParse(cboLocLoaiPhong.SelectedValue.ToString(), out int parsedLoai))
            {
                loai = parsedLoai;
            }
            string tinh = cboLocTinhTrang.SelectedItem?.ToString() ?? "Tất cả";
            LoadData(loai, tinh);
        }
    }
}
