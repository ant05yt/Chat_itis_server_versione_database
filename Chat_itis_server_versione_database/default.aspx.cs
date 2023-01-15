using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Web.Services.Description;
using System.Web.Http;
using System.Web.Script.Serialization;


namespace Chat_itis_server_versione_database
{
    public partial class _default : System.Web.UI.Page
    {
        SqlConnection cn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=ChatItis_db; " +"user id=sa;password=abacus");
        string[] strings;
        protected void Page_Load(object sender, EventArgs e)
        {
            string Query = "";
            string method = HttpContext.Current.Request.HttpMethod;
            if (method == "POST")
            {
                Query = (string) ReceivePost();
            }
            else 
            { 
                 Query = Request.QueryString["Q"];
            }           
            if (Query != null)
            {
               // try
                {
                    string[] separatingStrings = { "/-/" };
                    strings = Query.Split(separatingStrings, System.StringSplitOptions.None);
                    switch (strings[0])
                    {
                        case "001": accesso_utente(); break;
                        case "010": invio_messaggio(); break;
                        case "100": richiesta_messaggi(); break;                        
                        case "002": Crea_gruppo(); break;
                        case "020": invio_messaggio_gruppo(); break;
                        case "021": aggiungi_user_gruppo(); break;
                        case "022": elimina_user_gruppo(); break;
                        case "003": chiedi_immagine(); break;
                    }
                }
                //catch { };

            }
        }


        [HttpPost]
        public dynamic ReceivePost()
        {
            var request = HttpContext.Current.Request;
            string jsonData = new StreamReader(request.InputStream).ReadToEnd();

            // deserializzare i dati della richiesta
            JavaScriptSerializer js = new JavaScriptSerializer();
            dynamic data = js.Deserialize<dynamic>(jsonData);
            return data;

        }


        private void chiedi_immagine()
        {
            throw new NotImplementedException();
        }
        #region gestione_gruppi
        private void elimina_user_gruppo()
        {
            throw new NotImplementedException();
        }

        private void aggiungi_user_gruppo()
        {
            throw new NotImplementedException();
        }

        private void Crea_gruppo()
        {
            throw new NotImplementedException();
        }
        private void invio_messaggio_gruppo()
        {
            throw new NotImplementedException();
        }
        #endregion
        #region gestione_utenti
        private void invio_messaggio()
        { 
            string destinatario = strings[1];
            string mittente = strings[2];
            string codice_di_sicurezza = strings[3];
            string data = strings[4];
            string messaggio = strings[5];
            int pos= 0;
            SqlCommand cmd1 = new SqlCommand("SELECT id_messaggio FROM messaggi ORDER BY id_messaggio DESC", cn);
            cn.Open();
            SqlDataReader dr = cmd1.ExecuteReader();
            if (dr.Read())
            {
                pos = Convert.ToInt16(dr["id_messaggio"].ToString());
            }
            cn.Close();
            if (controlla_autenticazione(mittente, codice_di_sicurezza)==2)
            {
                try
                {
                cn.Open();
                cmd1 = new SqlCommand("insert into messaggi " + "( id_messaggio,destinatario, mittente, data, messaggio ) VALUES " + "( @n_id_messaggio,@n_destinatario, @n_mittente, @n_data, @n_messaggio )", cn);
                cmd1.Parameters.Add("@n_id_messaggio", SqlDbType.Int);
                cmd1.Parameters["@n_id_messaggio"].Value = pos+1;
                cmd1.Parameters.Add("@n_destinatario", SqlDbType.NChar);
                cmd1.Parameters["@n_destinatario"].Value = destinatario;
                cmd1.Parameters.Add("@n_mittente", SqlDbType.NChar);
                cmd1.Parameters["@n_mittente"].Value = mittente;
                cmd1.Parameters.Add("@n_data", SqlDbType.DateTime);
                cmd1.Parameters["@n_data"].Value = data;
                cmd1.Parameters.Add("@n_messaggio", SqlDbType.Text);
                cmd1.Parameters["@n_messaggio"].Value = messaggio;
                int NumeroRecordModificati = cmd1.ExecuteNonQuery();
                cn.Close();
                Response.Write("si");
                }
                catch(Exception e) 
                { 
                    if (e is System.Data.SqlClient.SqlException) 
                    {
                        Response.Write("Error");
                    } 
                };
                
            }

        }
            private void accesso_utente()
        {
            string nome = strings[1];
            string numero = strings[2];
            string password = strings[3];
            cn.Open();
            string query = "SELECT COUNT(*) FROM utenti";
            SqlCommand command = new SqlCommand(query, cn);
            int count = (int)command.ExecuteScalar();
            cn.Close();
            int esito = 0;
            if (count != 0)
            {
                esito = controlla_autenticazione(numero, password);
            }
            if(esito == 2)
            {
                Response.Write("si,password corretta");
            }else if (esito == 1)
            {
                Response.Write("si,password sbagliata");
            }
            else
            {
                Random random = new Random();
                int r = random.Next(100000, 999999);
                cn.Open();
                SqlCommand cmd1 = new SqlCommand("INSERT INTO utenti " + "( numero, codice,nickname ) VALUES " + "( @n_numero, @n_codice,@n_nickname )", cn);
                cmd1.Parameters.Add("@n_numero", SqlDbType.NChar);   cmd1.Parameters["@n_numero"].Value = numero;
                cmd1.Parameters.Add("@n_nickname", SqlDbType.NChar);    cmd1.Parameters["@n_nickname"].Value = nome;
                cmd1.Parameters.Add("@n_codice", SqlDbType.NChar);   cmd1.Parameters["@n_codice"].Value = r;
                //cmd1.Parameters.Add("@n_img_profilo", SqlDbType.Image); cmd1.Parameters["@n_img_profilo"].Value = null;
                int NumeroRecordModificati = cmd1.ExecuteNonQuery();
                cn.Close();
                Response.Write(("no/-/" + r).ToString());
            }               
        }
        #endregion
        #region gestione_base
        private void richiesta_messaggi()
        {
            string mittente = strings[1];
            string codice_di_sicurezza = strings[2];
		    if(controlla_autenticazione(mittente,codice_di_sicurezza)==2)
            {
			    string messaggi = "";
                List<int> id = new List<int>();
                SqlCommand cmd = new SqlCommand("SELECT id_messaggio, mittente, data, messaggio FROM messaggi WHERE destinatario = " + mittente + " ORDER BY data", cn);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    id.Add(Convert.ToInt16(dr["id_messaggio"].ToString()));
                    messaggi += dr["mittente"].ToString().Trim() + "/-/" +
                    dr["data"].ToString().Trim() + "/-/" +
                    dr["messaggio"].ToString().Trim()+"--///--";
                }
                dr.Close();
                cn.Close();
                SqlCommand cmd1 = new SqlCommand("DELETE FROM messaggi " + "WHERE destinatario = " + mittente, cn);
                cn.Open();
                int NumeroRecordModificati = cmd1.ExecuteNonQuery();
                cn.Close();
                Response.Write(messaggi);
		    }
            
        }
        private int controlla_autenticazione(string mittente, string codice_di_sicurezza) {
            int esiste =0 ;
            SqlCommand cmd3 = new SqlCommand("SELECT numero, codice FROM utenti WHERE numero='"+mittente+"'", cn); 
            cn.Open();
                SqlDataReader dr = cmd3.ExecuteReader();                
                while (dr.Read())
                {
                esiste=1;
                string si = dr["codice"].ToString().Trim();
                if ( si == codice_di_sicurezza)
                {
                    esiste = 2;
                } 
                }
                dr.Close();
            cn.Close();    
            return esiste;
        }
        #endregion
    }
}