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
using System.Threading;


namespace Chat_itis_server_versione_database
{
    public partial class _default : System.Web.UI.Page
    {
        SqlConnection cn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=ChatItis_db; " + "user id=sa;password=abacus");
        string[] strings;
        protected void Page_Load(object sender, EventArgs e)
        {
            string Query = "";
            string method = HttpContext.Current.Request.HttpMethod;
            if (method == "POST")
            {
                Query = (string)ReceivePost();
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
                        case "020": 
                            invio_messaggio_gruppo(strings[1], strings[2], strings[3], strings[4], strings[5],true);
                            Response.Write("si"); break;
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
            JavaScriptSerializer js = new JavaScriptSerializer();
            dynamic data = js.Deserialize<dynamic>(jsonData);
            return data;

        }

        #region gestione_immagini
        private void chiedi_immagine()
        {

        }
        #endregion
        #region gestione_gruppi
        private void elimina_user_gruppo()
        {
            string creatore = strings[1];
            string codice = strings[2];
            string gruppo = strings[3];
            string utente = strings[4];
            //controllo per vedere se sia il creatore del gruppo e controllo se l'accaunt esista
            if (controlla_dati(gruppo, creatore, codice))
            {
                try
                {
                    SqlCommand cmd1 = new SqlCommand("DELETE relazione_u_g WHERE numero_utente = @n_utente and Id_gruppo = @n_gruppo", cn);
                    cmd1.Parameters.Add("@n_utente", SqlDbType.NChar);
                    cmd1.Parameters["@n_utente"].Value = utente;
                    cmd1.Parameters.Add("@n_gruppo", SqlDbType.NChar);
                    cmd1.Parameters["@n_gruppo"].Value = gruppo;
                    cn.Open();
                    int NumeroRecordModificati = cmd1.ExecuteNonQuery();
                    cn.Close();
                    invio_messaggio_gruppo(gruppo, creatore, codice, DateTime.Now.ToString(), "il creatore ha  eliminato questo utente al gruppo:" + utente,false);
                    Response.Write("si");
                }
                catch(Exception e) 
                {
                    Response.Write("error");
                }
            }
        }
        private void aggiungi_user_gruppo()
        {
            string creatore = strings[1];
            string codice = strings[2];
            string gruppo = strings[3];
            string utente = strings[4];
            //controllo per vedere se sia il creatore del gruppo e controllo se l'accaunt esista
            if (controlla_dati(gruppo,creatore,codice))
            {
                try
                {
                    //inserisco l'utente 
                    SqlCommand cmd1 = new SqlCommand("INSERT INTO relazione_u_g ( numero_utente, Id_gruppo ) VALUES ( @n_utente, @n_gruppo )", cn);
                    cmd1.Parameters.Add("@n_utente", SqlDbType.NChar);
                    cmd1.Parameters["@n_utente"].Value = utente;
                    cmd1.Parameters.Add("@n_gruppo", SqlDbType.NChar);
                    cmd1.Parameters["@n_gruppo"].Value = gruppo;
                    cn.Open();
                    int NumeroRecordModificati = cmd1.ExecuteNonQuery();
                    cn.Close();
                    //invia il messaggio
                    invio_messaggio_gruppo(gruppo, creatore, codice, DateTime.Now.ToString(), "il creatore ha  aggiunto questo utente al gruppo:" + utente , false);
                    Response.Write("si");
                }
                catch (Exception e)
                {
                    Response.Write("error");
                }
            }
        }        
        private void Crea_gruppo()
        {
            string creatore = strings[1];
            string codice = strings[2];
            string nome_gruppo = strings[3];
            string descrizione = strings[4];
            //creo il gruppo 
            if (controlla_autenticazione(creatore, codice) == 2)
            {
                // Ottengo il numero come sottostringa a partire dal secondo carattere
                string inputString = "";
                SqlCommand cmd1 = new SqlCommand("SELECT id_gruppo FROM gruppi ORDER BY id_gruppo DESC", cn);
                cn.Open();
                SqlDataReader dr = cmd1.ExecuteReader();
                if (dr.Read())
                {
                    inputString = dr["id_gruppo"].ToString();
                }
                cn.Close();

                string numberString = inputString.Substring(1);
                int number = int.Parse(numberString);
                number++;
                string newString = "G" + number.ToString("D8");

                cmd1 = new SqlCommand("INSERT INTO gruppi ( id_gruppo , nome_gruppo, creatore, descrizione ) VALUES ( @n_id_gruppo,@nomegruppo, @n_creatore, @n_descrizione )", cn);
                cmd1.Parameters.Add("@n_id_gruppo", SqlDbType.NChar);
                cmd1.Parameters["@n_id_gruppo"].Value = newString;
                cmd1.Parameters.Add("@nomegruppo", SqlDbType.NChar);
                cmd1.Parameters["@nomegruppo"].Value = nome_gruppo;
                cmd1.Parameters.Add("@n_creatore", SqlDbType.NChar);
                cmd1.Parameters["@n_creatore"].Value = creatore;
                cmd1.Parameters.Add("@n_descrizione", SqlDbType.NChar);
                cmd1.Parameters["@n_descrizione"].Value = descrizione;
                cn.Open();
                int NumeroRecordModificati = cmd1.ExecuteNonQuery();
                cn.Close();
                //agggiungi creatire alla tabella
                cmd1 = new SqlCommand("INSERT INTO relazione_u_g ( numero_utente, Id_gruppo ) VALUES ( @n_utente, @n_gruppo )", cn);
                cmd1.Parameters.Add("@n_utente", SqlDbType.NChar);
                cmd1.Parameters["@n_utente"].Value = creatore;
                cmd1.Parameters.Add("@n_gruppo", SqlDbType.NChar);
                cmd1.Parameters["@n_gruppo"].Value = newString;
                cn.Open();
                NumeroRecordModificati = cmd1.ExecuteNonQuery();
                cn.Close();
                //invia il messaggio
                Thread.Sleep(10);
                invio_messaggio_gruppo(newString, creatore, codice, DateTime.Now.ToString(), "hai creato il gruppo",false);
                Response.Write("si/-/"+newString);
            }
        }
        private void invio_messaggio_gruppo(            string gruppo,
            string mittente,           
            string codice_di_sicurezza, 
            string data, 
            string messaggio ,
            bool a)
        {

            List<string> list = new List<string>();
            SqlCommand cmd1;       
            if(a)
                cmd1 = new SqlCommand("SELECT numero_utente FROM relazione_u_g WHERE (Id_gruppo = '"+ gruppo + "') AND (numero_utente <> "+mittente+")" , cn);
            else
                cmd1 = new SqlCommand("SELECT numero_utente FROM relazione_u_g WHERE (Id_gruppo = '" + gruppo + "')", cn);
            cn.Open();
            SqlDataReader dr = cmd1.ExecuteReader();
            while(dr.Read())
            {
                list.Add(dr["numero_utente"].ToString());
            }
            cn.Close();
            if (controlla_autenticazione(mittente, codice_di_sicurezza) == 2)
            {
                try
                {
                    cn.Open();
                    for (int i = 0; i < list.Count; i++)
                    {
                        cmd1 = new SqlCommand("insert into messaggi " + "( destinatario, mittente, data, messaggio, id_gruppo ) VALUES " + "( @n_destinatario, @n_mittente, @n_data, @n_messaggio, @n_id_gruppo )", cn);
                        cmd1.Parameters.Add("@n_destinatario", SqlDbType.NChar);
                        cmd1.Parameters["@n_destinatario"].Value = list[i];
                        cmd1.Parameters.Add("@n_mittente", SqlDbType.NChar);
                        cmd1.Parameters["@n_mittente"].Value = mittente;
                        cmd1.Parameters.Add("@n_data", SqlDbType.DateTime);
                        cmd1.Parameters["@n_data"].Value = data;
                        cmd1.Parameters.Add("@n_messaggio", SqlDbType.Text);
                        cmd1.Parameters["@n_messaggio"].Value = messaggio;
                        cmd1.Parameters.Add("@n_id_gruppo", SqlDbType.Text);
                        cmd1.Parameters["@n_id_gruppo"].Value = gruppo;
                        int NumeroRecordModificati = cmd1.ExecuteNonQuery();
                    }                    
                    cn.Close();

                }
                catch (Exception e)
                {
                    if (e is System.Data.SqlClient.SqlException)
                    {
                        Response.Write("Error");
                    }
                }

            }
        }
        public bool controlla_dati(string idGruppo, string idCreatore, string codiceIdentificazione)
        {
            bool isDataCorrect = false;
            string queryString = "SELECT * FROM gruppi WHERE id_gruppo = @idGruppo AND creatore = @n_creatore; SELECT * FROM Utenti WHERE numero = @n_creatore AND Codice = @codiceIdentificazione";
            SqlCommand command = new SqlCommand(queryString, cn);
            command.Parameters.AddWithValue("@idGruppo", idGruppo);
            command.Parameters.AddWithValue("@n_creatore", idCreatore);
            command.Parameters.AddWithValue("@codiceIdentificazione", codiceIdentificazione);
            cn.Open();
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows) // Se la query ha restituito almeno una riga
            {
                reader.NextResult(); // Sposta il puntatore alla prossima tabella di risultati

                if (reader.HasRows) // Se la seconda query ha restituito almeno una riga
                {
                    isDataCorrect = true; // I dati sono corretti
                }
            }
            reader.Close();
            cn.Close(); 
            return isDataCorrect;
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
            SqlCommand cmd1;
            if (controlla_autenticazione(mittente, codice_di_sicurezza)==2)
            {
                try
                {
                cn.Open();
                cmd1 = new SqlCommand("insert into messaggi " + "(destinatario, mittente, data, messaggio ) VALUES " + "(@n_destinatario, @n_mittente, @n_data, @n_messaggio )", cn);
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
                }
                
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
                SqlCommand cmd = new SqlCommand("SELECT * FROM messaggi WHERE destinatario = " + mittente + " ORDER BY data", cn);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    id.Add(Convert.ToInt16(dr["id_messaggio"].ToString()));
                    messaggi += dr["mittente"].ToString().Trim() + "/-/" +
                    dr["data"].ToString().Trim() + "/-/" +
                    dr["messaggio"].ToString().Trim() + "/-/" +
                    dr["id_gruppo"].ToString().Trim() + "--///--";
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
        private int controlla_autenticazione( string mittente, string codice_di_sicurezza) {
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