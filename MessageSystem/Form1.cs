using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Xml;
using System.IO;

namespace MessageSystem
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        class Global
        {
            public static string ConnString { get; set; }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SqlConnection cnn = new SqlConnection(Global.ConnString);

            try {
                cnn.Open();
                MessageBox.Show("Połączenie nawiązane pomyślnie.");
                dataGridView1.DataSource = ReloadTable();
                cnn.Close();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string config = "config.xml";

            if (!File.Exists(config))
                File.WriteAllText(config, "<Credentials>\n<address>localhost\\SQLEXPRESS</address>\n<database>hosting</database>\n<user>sa</user>\n<password></password>"
                    + "\n<wauth>True</wauth>\n</Credentials>");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(File.ReadAllText(config));

            textBox8.Text = doc.DocumentElement.SelectSingleNode("/Credentials/address").InnerText;
            textBox9.Text = doc.DocumentElement.SelectSingleNode("/Credentials/database").InnerText;
            textBox5.Text = doc.DocumentElement.SelectSingleNode("/Credentials/user").InnerText;
            textBox6.Text = doc.DocumentElement.SelectSingleNode("/Credentials/password").InnerText;

            Global.ConnString = "Data Source=" + textBox8.Text + ";Initial Catalog=" + textBox9.Text + ";User ID=" + textBox5.Text;

            if (doc.DocumentElement.SelectSingleNode("/Credentials/wauth").InnerText == "True")
            {
                Global.ConnString += ";Integrated Security=True;";
                textBox5.Enabled = false;
                textBox6.Enabled = false;
                checkBox1.Checked = true;
            }
            else
            {
                Global.ConnString += ";Password=" + doc.DocumentElement.SelectSingleNode("/Credentials/password").InnerText;
                textBox5.Enabled = true;
                textBox6.Enabled = true;
                checkBox1.Checked = false;
            }
        }

        private void credentials_TextChanged(object sender, EventArgs e)
        {
            Global.ConnString = "Data Source=" + textBox8.Text + ";Initial Catalog=" + textBox9.Text + ";User ID=" + textBox5.Text;
                
            if(checkBox1.Checked)
            {
                Global.ConnString += ";Integrated Security=True;";
                textBox5.Enabled = false;
                textBox6.Enabled = false;
            } else
            {
                Global.ConnString += ";Password=" + textBox6.Text;
                textBox5.Enabled = true;
                textBox6.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((textBox1.Text == "") || (textBox2.Text == ""))
            {
                MessageBox.Show("Pola nie mogą być puste.");
                return;
            }

            SqlConnection cnn = new SqlConnection(Global.ConnString);

            try
            {
                cnn.Open();

                SqlCommand command = new SqlCommand("INSERT INTO dbo.messages (title,message,date) VALUES (@title,@message, @date)", cnn);
                command.Parameters.AddWithValue("@title", textBox1.Text);
                command.Parameters.AddWithValue("@message", textBox2.Text);
                command.Parameters.AddWithValue("@date", DateTime.Now);
                command.ExecuteNonQuery();

                cnn.Close();
                MessageBox.Show("Dodano pomyślnie nową wiadomość do bazy.");
                dataGridView1.DataSource = ReloadTable();

                textBox1.Text = "";
                textBox2.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        public static DataTable ReloadTable()
        {
            SqlConnection cnn = new SqlConnection(Global.ConnString);

            try
            {
                cnn.Open();

                var dataAdapter = new SqlDataAdapter("SELECT id, title, message, date FROM dbo.messages", cnn);
                var commandBuilder = new SqlCommandBuilder(dataAdapter);

                cnn.Close();

                var ds = new DataSet();
                dataAdapter.Fill(ds);
                return ds.Tables[0];
            }
            catch
            {
                return null;
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if(dataGridView1.SelectedRows.Count == 0)
                return;

            SqlConnection cnn = new SqlConnection(Global.ConnString);

            try
            {
                cnn.Open();

                SqlCommand command = new SqlCommand("SELECT id, title, message, date FROM dbo.messages WHERE id = @id", cnn);
                command.Parameters.AddWithValue("@id", dataGridView1.SelectedRows[0].Cells["id"].Value);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        textBox3.Text = String.Format("{0}", reader["title"]);
                        textBox4.Text = String.Format("{0}", reader["message"]);
                        textBox7.Text = String.Format("{0}", reader["date"]);
                    }
                }

                cnn.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if ((dataGridView1.SelectedRows.Count == 0) || (dataGridView1.CurrentCell == null))
                return;

            SqlConnection cnn = new SqlConnection(Global.ConnString);

            try
            {
                cnn.Open();

                SqlCommand command = new SqlCommand("DELETE FROM dbo.messages WHERE id = @id", cnn);
                command.Parameters.AddWithValue("@id", dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells["id"].Value);
                command.ExecuteNonQuery();

                dataGridView1.ClearSelection();
                dataGridView1.DataSource = ReloadTable();
                textBox3.Text = "";
                textBox4.Text = "";
                textBox7.Text = "";

                cnn.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if( tabControl1.SelectedTab == tabPage2 )
                dataGridView1.DataSource = ReloadTable();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            string config = "config.xml";

            File.WriteAllText(config, "<Credentials>\n<address>" + textBox8.Text + "</address>\n<database>" + textBox9.Text + "</database>\n<user>" + textBox5.Text
                + "</user>\n<password>" + textBox6.Text + "</password>\n<wauth>" + checkBox1.Checked.ToString() + "</wauth>\n</Credentials>");
        }
    }
}
