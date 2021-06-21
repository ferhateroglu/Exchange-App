﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Exchange_App
{
    public partial class UserControlAlimSatim : UserControl
    {
        string aliciBakiye, AliciUrunMiktar, saticiBakiye, saticiUrunMiktar,ogeID;
        double alisverisTutar;
        DataTable dt = new DataTable();

        public UserControlAlimSatim()
        {
            InitializeComponent();
        }

        SqlConnection baglanti = new SqlConnection(@"Data Source=DESKTOP-2BOGKJG;Initial Catalog=yazilimYapimi;Integrated Security=True");

        // Satın alma butonu
        private void btnAl_Click(object sender, EventArgs e)
        {
            double alinacakMiktar = Convert.ToInt32(txtMiktar.Text);
            double satisFiyat;
            int saticiStok;
            while (alinacakMiktar != 0)
            {
                string saticiId = dataGridView1.Rows[0].Cells[0].Value.ToString();
                satisFiyat = Convert.ToInt32(dataGridView1.Rows[0].Cells[3].Value);
                saticiStok = Convert.ToInt32(dataGridView1.Rows[0].Cells[2].Value);

                aliciBilgiGetir();
                saticiBilgiGetir(saticiId);
                if (aliciBakiye == "0")
                {
                    MessageBox.Show("yetersiz bakiye");
                    alinacakMiktar = 0;
                }
                else if(alinacakMiktar <= saticiStok)
                {           
                    alisverisTutar = alinacakMiktar * satisFiyat;
                    aliciBakiye = (Convert.ToDouble(aliciBakiye) - alisverisTutar).ToString();
                    AliciUrunMiktar = (Convert.ToDouble(AliciUrunMiktar) + Convert.ToDouble(alinacakMiktar)).ToString();
                    saticiBakiye = (Convert.ToDouble(saticiBakiye) + alisverisTutar).ToString();
                    saticiUrunMiktar = (Convert.ToDouble(saticiUrunMiktar) - Convert.ToDouble(alinacakMiktar)).ToString();
                    komisyonHesapla();
                    bilgiGuncelle(FrmLogin.id,AliciUrunMiktar,aliciBakiye);
                    bilgiGuncelle(saticiId,saticiUrunMiktar,saticiBakiye);
                    listele();
                    raporEkle(alinacakMiktar);
                    alinacakMiktar = 0;
                }
                else if( alinacakMiktar > saticiStok)
                {
                    alisverisTutar = saticiStok * satisFiyat;
                    aliciBakiye = (Convert.ToDouble(aliciBakiye) - alisverisTutar).ToString();
                    AliciUrunMiktar = (Convert.ToDouble(AliciUrunMiktar) + Convert.ToDouble(saticiStok)).ToString();
                    saticiBakiye = (Convert.ToDouble(saticiBakiye) + alisverisTutar).ToString();
                    saticiUrunMiktar = "0";
                    alinacakMiktar -= saticiStok;
                    //
                    bilgiGuncelle(FrmLogin.id, AliciUrunMiktar, aliciBakiye);
                    bilgiGuncelle(saticiId, saticiUrunMiktar, saticiBakiye);
                    listele();
                    raporEkle(saticiStok);
                }
            }
            txtMiktar.Clear();
            txtFiyat.Clear();
            checkBox1.Checked = false;
            MessageBox.Show("Alım işlemi başarılı");
        }

        private void komisyonHesapla()
        {
            double komisyon = alisverisTutar / 100;
            aliciBakiye = (Convert.ToDouble(aliciBakiye) - komisyon).ToString();
            baglanti.Open();
            SqlCommand komut = new SqlCommand("select ogeMiktar from KullaniciOgeleri where kullaniciID=6 and ogeID=4",baglanti);
            string mevcutpara = komut.ExecuteScalar().ToString();
            mevcutpara = mevcutpara.Replace(".", ",");
            mevcutpara = (Convert.ToDouble(mevcutpara) + komisyon).ToString();
            baglanti.Close();

            baglanti.Open();
            SqlCommand komutGuncelle = new SqlCommand("update KullaniciOgeleri set ogeMiktar=@p1 where kullaniciID=6 and ogeID=4", baglanti);
            komutGuncelle.Parameters.AddWithValue("@p1", mevcutpara);
            komutGuncelle.ExecuteNonQuery();
            baglanti.Close();
        }
        private void raporEkle(double miktar)
        {
            baglanti.Open();
            SqlCommand komut = new SqlCommand("insert into Rapor (kullaniciID,ogeID,miktar,tarih) values(@p1,@p2,@p3,@p4)", baglanti);
            komut.Parameters.AddWithValue("@p1",FrmLogin.id);
            komut.Parameters.AddWithValue("@p2",ogeID);
            komut.Parameters.AddWithValue("@p3",miktar);
            komut.Parameters.AddWithValue("@p4",DateTime.Now);
            komut.ExecuteNonQuery();
            baglanti.Close();
        }
        private void bilgiGuncelle(string id,string urunMiktar,string para)
        {
            baglanti.Open();
            SqlCommand komutoge = new SqlCommand("update KullaniciOgeleri set ogeMiktar=@p1 where kullaniciID=@p2 and ogeID=@p3", baglanti);
            komutoge.Parameters.AddWithValue("@p1", urunMiktar);
            komutoge.Parameters.AddWithValue("@p2",id);
            komutoge.Parameters.AddWithValue("@p3", ogeID);
            komutoge.ExecuteNonQuery();
            baglanti.Close();

            baglanti.Open();
            SqlCommand komutPara = new SqlCommand("update KullaniciOgeleri set ogeMiktar=@p4 where kullaniciID=@p5 and ogeID=@p6", baglanti);
            komutPara.Parameters.AddWithValue("@p4", para);
            komutPara.Parameters.AddWithValue("@p5", id);
            komutPara.Parameters.AddWithValue("@p6", "4");
            komutPara.ExecuteNonQuery();
            baglanti.Close();
        }


        // Comboboxtaki ürün seçime göre satıcıları listeler
        private void cmbUrun_SelectedIndexChanged(object sender, EventArgs e)
        {
            dt.Clear();
            if(cmbUrun.SelectedIndex == 0)
            {
                arpaListele();
                ogeID = "1";

            }
            else if (cmbUrun.SelectedIndex == 1)
            {
                bugdayListele();
                ogeID = "2";
            }
            else if (cmbUrun.SelectedIndex == 2)
            {
                pamukListele();
                ogeID = "3";
            }
        }

        private void listele()
        {
            dt.Clear();
            if (ogeID == "1")
            {
                arpaListele();
            }
            else if (ogeID == "2")
            {
                bugdayListele();
            }
            else if( ogeID == "3")
            {
                pamukListele();
            }
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                txtFiyat.Visible = true;
                label4.Visible = true;
                panel3.Visible = true;
            }
            else
            {
                txtFiyat.Visible = false;
                label4.Visible = false;
                panel3.Visible = false;
            }
        }

        private void txtFiyat_TextChanged(object sender, EventArgs e)
        {
            DataView dv = dt.DefaultView;
            dv.RowFilter = "ogeFiyat LIKE '" + txtFiyat.Text + "%'";
            dataGridView1.DataSource = dv;
        }

        //Arpa listeleme
        private void arpaListele()
        {
            string kullaniciId = FrmLogin.id;
            baglanti.Open();
            SqlDataAdapter da = new SqlDataAdapter("select K.kullaniciID,O.ogeAdi,Ko.ogeMiktar,Ko.ogeFiyat from Kullanicilar K inner join KullaniciOgeleri Ko on K.kullaniciID = Ko.kullaniciID inner join KullaniciTipleri Kt on Kt.kullaniciID = K.kullaniciID inner join Ogeler O on O.ogeID = Ko.ogeID where Kt.kullanicitipAdi = 'user' and O.ogeID = 1 and  K.kullaniciID !=@p1 and KO.ogeMiktar !=0 order by Ko.ogeFiyat asc", baglanti);
            da.SelectCommand.Parameters.AddWithValue("@p1", kullaniciId);
            da.Fill(dt);
            dataGridView1.DataSource = dt;
            baglanti.Close();
        }




        //Buğday Listeleme
        private void bugdayListele()
        {
            string kullaniciId = FrmLogin.id;   
            baglanti.Open();
            SqlDataAdapter da = new SqlDataAdapter("select K.kullaniciID,O.ogeAdi,Ko.ogeMiktar,Ko.ogeFiyat from Kullanicilar K inner join KullaniciOgeleri Ko on K.kullaniciID = Ko.kullaniciID inner join KullaniciTipleri Kt on Kt.kullaniciID = K.kullaniciID inner join Ogeler O on O.ogeID = Ko.ogeID where Kt.kullanicitipAdi = 'user' and O.ogeID = 2 and  K.kullaniciID !=@p1 and KO.ogeMiktar !=0 order by Ko.ogeFiyat asc", baglanti);
            da.SelectCommand.Parameters.AddWithValue("@p1", kullaniciId);
            da.Fill(dt);
            dataGridView1.DataSource = dt;
            baglanti.Close();
        }

        // Pamuk Listeleme
        private void pamukListele()
        {
            string kullaniciId = FrmLogin.id;
            baglanti.Open();
            SqlDataAdapter da = new SqlDataAdapter("select K.kullaniciID,O.ogeAdi,Ko.ogeMiktar,Ko.ogeFiyat from Kullanicilar K inner join KullaniciOgeleri Ko on K.kullaniciID = Ko.kullaniciID inner join KullaniciTipleri Kt on Kt.kullaniciID = K.kullaniciID inner join Ogeler O on O.ogeID = Ko.ogeID where Kt.kullanicitipAdi = 'user' and O.ogeID = 3 and  K.kullaniciID !=@p1 and KO.ogeMiktar !=0 order by Ko.ogeFiyat asc", baglanti);
            da.SelectCommand.Parameters.AddWithValue("@p1", kullaniciId);
            da.Fill(dt);
            dataGridView1.DataSource = dt;
            baglanti.Close();
        }
        

        // Alım İşlemi gerçekleşmeden önce alıcının elinde bulunan ürün miktarı ve bakiye
        private void aliciBilgiGetir()
        {
            string kullaniciId = FrmLogin.id;
            baglanti.Open();
            SqlCommand komutbakiye = new SqlCommand("select ogeMiktar  from KullaniciOgeleri where kullaniciID =@p1 and ogeID=4", baglanti);
            komutbakiye.Parameters.AddWithValue("@p1", kullaniciId);
            aliciBakiye =komutbakiye.ExecuteScalar().ToString();
            baglanti.Close();

            if (cmbUrun.SelectedIndex == 0)
            {
                baglanti.Open();
                SqlCommand komutUrun = new SqlCommand("select ogeMiktar  from KullaniciOgeleri where kullaniciID =@p1 and ogeID=1", baglanti);
                komutUrun.Parameters.AddWithValue("@p1", kullaniciId);
                AliciUrunMiktar = komutUrun.ExecuteScalar().ToString();
                baglanti.Close();
            }
            else if(cmbUrun.SelectedIndex == 1)
            {
                baglanti.Open();
                SqlCommand komutUrun = new SqlCommand("select ogeMiktar  from KullaniciOgeleri where kullaniciID =@p1 and ogeID=2", baglanti);
                komutUrun.Parameters.AddWithValue("@p1", kullaniciId);
                AliciUrunMiktar = komutUrun.ExecuteScalar().ToString();
                baglanti.Close();
            }
            else if (cmbUrun.SelectedIndex == 2)
            {
                baglanti.Open();
                SqlCommand komutUrun = new SqlCommand("select ogeMiktar  from KullaniciOgeleri where kullaniciID =@p1 and ogeID=3", baglanti);
                komutUrun.Parameters.AddWithValue("@p1", kullaniciId);
                AliciUrunMiktar = komutUrun.ExecuteScalar().ToString();
                baglanti.Close();
            }
        }

        // Alım İşlemi gerçekleşmeden önce satıcının elinde bulunan ürün miktarı ve bakiye
        private void saticiBilgiGetir(string saticiId)
        {
            string kullaniciId = saticiId;
            baglanti.Open();
            SqlCommand komutbakiye = new SqlCommand("select ogeMiktar  from KullaniciOgeleri where kullaniciID =@p1 and ogeID=4", baglanti);
            komutbakiye.Parameters.AddWithValue("@p1", kullaniciId);
            if(komutbakiye.ExecuteScalar() == null)
            {
                saticiBakiye = "0";
            }
            else
            {
                saticiBakiye = komutbakiye.ExecuteScalar().ToString();
            }
            baglanti.Close();

            if (cmbUrun.SelectedIndex == 0)
            {
                baglanti.Open();
                SqlCommand komutUrun = new SqlCommand("select ogeMiktar  from KullaniciOgeleri where kullaniciID =@p1 and ogeID=1", baglanti);
                komutUrun.Parameters.AddWithValue("@p1", kullaniciId);
                saticiUrunMiktar = komutUrun.ExecuteScalar().ToString();
                baglanti.Close();
            }
            else if (cmbUrun.SelectedIndex == 1)
            {
                baglanti.Open();
                SqlCommand komutUrun = new SqlCommand("select ogeMiktar  from KullaniciOgeleri where kullaniciID =@p1 and ogeID=2", baglanti);
                komutUrun.Parameters.AddWithValue("@p1", kullaniciId);
                saticiUrunMiktar = komutUrun.ExecuteScalar().ToString();
                baglanti.Close();
            }
            else if (cmbUrun.SelectedIndex == 2)
            {
                baglanti.Open();
                SqlCommand komutUrun = new SqlCommand("select ogeMiktar  from KullaniciOgeleri where kullaniciID =@p1 and ogeID=3", baglanti);
                komutUrun.Parameters.AddWithValue("@p1", kullaniciId);
                saticiUrunMiktar = komutUrun.ExecuteScalar().ToString();
                baglanti.Close();
            }
        }
    }
}
