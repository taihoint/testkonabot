using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Text;
using System.Collections;
using System.Data;
using System.Diagnostics;
using Bot_Application1.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Web.Configuration;

namespace Bot_Application1.DB 
{

    public class DbConnect
    {
        static Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("/testkonabot");
        const string CONSTRINGNAME = "conString";
        ConnectionStringSettings connStr = rootWebConfig.ConnectionStrings.ConnectionStrings[CONSTRINGNAME];

        //string connStr = "Data Source=faxtimedb.database.windows.net;Initial Catalog=taihoML;User ID=faxtime;Password=test2016!;";
        //string connStr = "Data Source= hyundaidb.database.windows.net;Initial Catalog=taihoML;User ID=taihoinst;Password=taiho123@;";
        //string connStr = "Data Source=10.6.222.21,1433;Initial Catalog=konadb;User ID=konadb;Password=Didwoehd20-9!;";
        StringBuilder sb = new StringBuilder();

        public void ConnectDb()
        {
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connStr.ConnectionString);
                conn.Open();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }

        }

        public List<LuisList> SelectLuis(string intent, String entities, int appID)
        {
            SqlDataReader rdr = null;
            List<LuisList> luis = new List<LuisList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText = "sp_selectdialogid_new";
                //cmd.CommandText = "sp_selectdialogid_new2";

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@intent", intent);
                cmd.Parameters.AddWithValue("@entities", entities);
                cmd.Parameters.AddWithValue("@appID", appID);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    int appId = Convert.ToInt32(rdr["APP_ID"]);
                    LuisList luis1 = new LuisList();
                    luis1.dlgId = dlgId;
                    luis1.appId = appId;
                    luis.Add(luis1);
                }
                //Debug.WriteLine("luisluisluisluis : " + luis.Count);
            }
            return luis;
        }

        public List<DialogList> SelectInitDialog()
        {
            SqlDataReader rdr = null;
            List<DialogList> dialog = new List<DialogList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT DLG_ID, DLG_NM, DLG_MENT, DLG_LANG FROM TBL_DLG WHERE DLG_TYPE = '1' AND USE_YN = 'Y' AND DLG_ID > 999 ORDER BY ORDER_NO ASC";

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string dlgNm = rdr["DLG_NM"] as string;
                    string dlgMent = rdr["DLG_MENT"] as string;
                    string dlgLang = rdr["DLG_LANG"] as string;

                    DialogList dlg = new DialogList();
                    dlg.dlgId = dlgId;
                    dlg.dlgNm = dlgNm;
                    dlg.dlgMent = dlgMent;
                    dlg.dlgLang = dlgLang;

                    dialog.Add(dlg);
                }
            }
            return dialog;
        }


        public List<DialogList> SelectDialog(int dlgID)
        {
            SqlDataReader rdr = null;
            List<DialogList> dialog = new List<DialogList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                //cmd.CommandText = "SELECT DLG_ID, DLG_NM, DLG_MENT, DLG_LANG FROM TBL_DLG WHERE DLG_ID = @dlgID AND USE_YN = 'Y' AND DLG_ID > 999";
                cmd.CommandText = "SELECT DLG_ID, DLG_NM, DLG_MENT, DLG_LANG, APP_ID FROM TBL_DLG WHERE DLG_ID = @dlgID AND USE_YN = 'Y' AND DLG_ID > 999 order by order_no";

                cmd.Parameters.AddWithValue("@dlgID", dlgID);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string dlgNm = rdr["DLG_NM"] as string;
                    string dlgMent = rdr["DLG_MENT"] as string;
                    string dlgLang = rdr["DLG_LANG"] as string;
                    int appId = Convert.ToInt32(rdr["APP_ID"]);

                    DialogList dlg = new DialogList();
                    dlg.dlgId = dlgId;
                    dlg.dlgNm = dlgNm;
                    dlg.dlgMent = dlgMent;
                    dlg.dlgLang = dlgLang;
                    dlg.appId = appId;

                    dialog.Add(dlg);
                }
            }
            return dialog;
        }

        public List<CardList> SelectDialogCard(int dlgID)
        {
            SqlDataReader rdr = null;
            List<CardList> dialogCard = new List<CardList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT CARD_ID, DLG_ID, CARD_TYPE, CARD_TITLE, CARD_SUBTITLE, CARD_TEXT, CARD_DIVISION, CARD_VALUE FROM TBL_DLG_CARD WHERE DLG_ID = @dlgID AND USE_YN = 'Y' AND DLG_ID > 999";

                cmd.Parameters.AddWithValue("@dlgID", dlgID);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    int cardId = Convert.ToInt32(rdr["CARD_ID"]);
                    int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string cardType = rdr["CARD_TYPE"] as string;
                    string cardText = rdr["CARD_TEXT"] as string;
                    string cardTitle = rdr["CARD_TITLE"] as string;
                    string cardSubTitle = rdr["CARD_SUBTITLE"] as string;
                    string cardDivision = rdr["CARD_DIVISION"] as string;
                    string cardValue = rdr["CARD_VALUE"] as string;

                    CardList dlgCard = new CardList();
                    dlgCard.cardId = cardId;
                    dlgCard.dlgId = dlgId;
                    dlgCard.cardType = cardType;
                    dlgCard.cardText = cardText;
                    dlgCard.cardTitle = cardTitle;
                    dlgCard.cardSubTitle = cardSubTitle;
                    dlgCard.cardDivision = cardDivision;
                    dlgCard.cardValue = cardValue;

                    dialogCard.Add(dlgCard);
                }
            }
            return dialogCard;
        }


        public int SelectDialogCardCnt(int dlgID)
        {
            SqlDataReader rdr = null;
            int cardCnt = 0;

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT COUNT(*) CNT FROM TBL_DLG_CARD WHERE DLG_ID = @dlgID AND USE_YN = 'Y' AND DLG_ID > 999";

                cmd.Parameters.AddWithValue("@dlgID", dlgID);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    cardCnt = Convert.ToInt32(rdr["CNT"]);
                }
            }
            return cardCnt;
        }



        public List<CardList> SelectDialogCardFB(int dlgID, int pageCnt)
        {
            SqlDataReader rdr = null;
            List<CardList> dialogCard = new List<CardList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += " SELECT  ";
                cmd.CommandText += " 	CARD_ID ,DLG_ID, CARD_TYPE, CARD_TITLE, CARD_SUBTITLE, CARD_TEXT, CARD_DIVISION, CARD_VALUE  ";
                cmd.CommandText += "  FROM TBL_DLG_CARD  ";
                cmd.CommandText += " WHERE DLG_ID = @dlgID ";
                cmd.CommandText += "   AND USE_YN = 'Y'  ";
                cmd.CommandText += "   AND DLG_ID > 999 ";
                cmd.CommandText += " ORDER BY CARD_ID ";
                cmd.CommandText += " OFFSET @pageCnt ROWS ";
                cmd.CommandText += " FETCH NEXT 10 ROWS ONLY ";

                cmd.Parameters.AddWithValue("@dlgID", dlgID);
                cmd.Parameters.AddWithValue("@pageCnt", pageCnt);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    int cardId = Convert.ToInt32(rdr["CARD_ID"]);
                    int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string cardType = rdr["CARD_TYPE"] as string;
                    string cardText = rdr["CARD_TEXT"] as string;
                    string cardTitle = rdr["CARD_TITLE"] as string;
                    string cardSubTitle = rdr["CARD_SUBTITLE"] as string;
                    string cardDivision = rdr["CARD_DIVISION"] as string;
                    string cardValue = rdr["CARD_VALUE"] as string;

                    CardList dlgCard = new CardList();
                    dlgCard.cardId = cardId;
                    dlgCard.dlgId = dlgId;
                    dlgCard.cardType = cardType;
                    dlgCard.cardText = cardText;
                    dlgCard.cardTitle = cardTitle;
                    dlgCard.cardSubTitle = cardSubTitle;
                    dlgCard.cardDivision = cardDivision;
                    dlgCard.cardValue = cardValue;

                    dialogCard.Add(dlgCard);
                }
            }
            return dialogCard;
        }


        public List<ButtonList> SelectBtn(int dlgID, int cardID, string YN)
        {
            SqlDataReader rdr = null;
            List<ButtonList> button = new List<ButtonList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                if(YN == "Y")
                {
                    cmd.CommandText = "SELECT DLG_ID, CARD_ID, BTN_ID, BTN_TYPE, BTN_TITLE, BTN_CONTEXT FROM TBL_DLG_BTN WHERE DLG_ID = @dlgID AND CARD_ID = @cardID AND USE_YN = 'Y' AND DLG_ID > 999 AND FB_USE_YN = @YN";
                }
                else
                {
                    cmd.CommandText = "SELECT DLG_ID, CARD_ID, BTN_ID, BTN_TYPE, BTN_TITLE, BTN_CONTEXT FROM TBL_DLG_BTN WHERE DLG_ID = @dlgID AND CARD_ID = @cardID AND USE_YN = 'Y' AND DLG_ID > 999 ";
                }

                cmd.Parameters.AddWithValue("@dlgID", dlgID);
                cmd.Parameters.AddWithValue("@cardID", cardID);
                cmd.Parameters.AddWithValue("@YN", YN);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    int btnId = Convert.ToInt32(rdr["BTN_ID"]);
                    int cardId = Convert.ToInt32(rdr["CARD_ID"]);
                    int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string btnType = rdr["BTN_TYPE"] as string;
                    string btnTitle = rdr["BTN_TITLE"] as string;
                    string btnContext = rdr["BTN_CONTEXT"] as string;

                    ButtonList btn = new ButtonList();
                    btn.btnId = btnId;
                    btn.cardId = cardId;
                    btn.dlgId = dlgId;
                    btn.btnType = btnType;
                    btn.btnTitle = btnTitle;
                    btn.btnContext = btnContext;

                    button.Add(btn);
                }
            }
            return button;
        }

        public List<ImagesList> SelectImage(int dlgID, int cardID)
        {
            SqlDataReader rdr = null;
            List<ImagesList> image = new List<ImagesList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT IMG_ID, DLG_ID, CARD_ID, IMG_URL FROM TBL_DLG_IMG WHERE DLG_ID = @dlgID AND CARD_ID = @cardID AND USE_YN = 'Y' AND DLG_ID > 999";

                cmd.Parameters.AddWithValue("@dlgID", dlgID);
                cmd.Parameters.AddWithValue("@cardID", cardID);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    int imgId = Convert.ToInt32(rdr["IMG_ID"]);
                    int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    int cardId = Convert.ToInt32(rdr["CARD_ID"]);
                    string imgUrl = rdr["IMG_URL"] as string;

                    ImagesList img = new ImagesList();
                    img.dlgId = dlgId;
                    img.imgId = imgId;
                    img.cardId = cardId;
                    img.imgUrl = imgUrl;

                    image.Add(img);
                }
            }
            return image;
        }

        public List<MediaList> SelectMedia(int dlgID, int cardID)
        {
            SqlDataReader rdr = null;
            List<MediaList> media = new List<MediaList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT MEDIA_ID, DLG_ID, CARD_ID, MEDIA_URL FROM TBL_DLG_MEDIA WHERE DLG_ID = @dlgID AND CARD_ID = @cardID AND USE_YN = 'Y' AND DLG_ID > 999";

                cmd.Parameters.AddWithValue("@dlgID", dlgID);
                cmd.Parameters.AddWithValue("@cardID", cardID);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    int mediaId = Convert.ToInt32(rdr["MEDIA_ID"]);
                    int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    int cardId = Convert.ToInt32(rdr["CARD_ID"]);
                    string mediaUrl = rdr["MEDIA_URL"] as string;

                    MediaList med = new MediaList();
                    med.dlgId = dlgId;
                    med.cardId = cardId;
                    med.mediaId = mediaId;
                    med.mediaUrl = mediaUrl;


                    media.Add(med);
                }
            }
            return media;
        }


        public string SelectChgMsg(string oldMsg)
        {
            SqlDataReader rdr = null;
            string newMsg = "";

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "	SELECT FIND.CHG  CHG_WORD FROM(    					    ";
                cmd.CommandText += "	SELECT                                                  ";
                cmd.CommandText += "			CASE WHEN LEN(ORG_WORD) = LEN(@oldMsg)          ";
                cmd.CommandText += "				THEN CHARINDEX(ORG_WORD, @oldMsg)           ";
                cmd.CommandText += "				ELSE 0                                      ";
                cmd.CommandText += "				END AS FIND_NUM,                            ";
                cmd.CommandText += "				REPLACE(@oldMsg, ORG_WORD, CHG_WORD) CHG    ";
                cmd.CommandText += "	  FROM TBL_WORD_CHG_DICT                                ";
                cmd.CommandText += "	  ) FIND                                                ";
                cmd.CommandText += "	  WHERE FIND.FIND_NUM > 0                               ";





                cmd.Parameters.AddWithValue("@oldMsg", oldMsg);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    newMsg = rdr["CHG_WORD"] as string;
                }
            }
            return newMsg;
        }

        //금칙어
        public string SelectBannedWordAnswerMsg(string msg)
        {
            SqlDataReader rdr = null;
            string answerMsg = "";

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += " SELECT                                                                                													";
                cmd.CommandText += "         TOP 1 BANNED_WORD_ANSWER, (SELECT TOP 1 BANNED_WORD FROM TBL_BANNED_WORD_LIST WHERE CHARINDEX(BANNED_WORD, @msg) > 0) AS BANNED_WORD ,BANNED_WORD_TYPE ";
                cmd.CommandText += "   FROM TBL_BANNED_WORD_ANSWER                                                                                                          ";
                cmd.CommandText += "  WHERE BANNED_WORD_TYPE =                                                                                                              ";
                cmd.CommandText += " (                                                                                                                                      ";
                cmd.CommandText += "    SELECT CASE WHEN SUM(CASE WHEN BANNED_WORD_TYPE = 1 THEN CHARINDEX(A.BANNED_WORD, @msg) END) > 0 THEN 1                    			";
                cmd.CommandText += "           WHEN SUM(CASE WHEN BANNED_WORD_TYPE = 2 THEN CHARINDEX(A.BANNED_WORD, @msg) END) > 0 THEN 2                         			";
                cmd.CommandText += "          WHEN SUM(CASE WHEN BANNED_WORD_TYPE = 3 THEN CHARINDEX(A.BANNED_WORD, @msg) END) > 0 THEN 3                          			";
                cmd.CommandText += "          END                                                                                                                           ";
                cmd.CommandText += "    FROM TBL_BANNED_WORD_LIST A                                                                                                         ";
                cmd.CommandText += " ) AND BANNED_WORD_TYPE IN (1,2,3)                                                                                                        ";
                cmd.CommandText += " ORDER BY NEWID()                                                                                                                       ";

                cmd.Parameters.AddWithValue("@msg", msg);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (rdr.Read())
                {
                    answerMsg = rdr["BANNED_WORD_ANSWER"] + "@@" + rdr["BANNED_WORD"] + "@@" + rdr["BANNED_WORD_TYPE"];
                }
            }
            return answerMsg;
        }



        public int insertHistory(string userNumber, string customerCommentKR, string customerCommentEN, string chatbotCommentCode, string channel, int responseTime, int appID)
        {
            //SqlDataReader rdr = null;
            int result;
            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += " INSERT INTO TBL_HISTORY_QUERY ";
                cmd.CommandText += " (USER_NUMBER, CUSTOMER_COMMENT_KR, CUSTOMER_COMMENT_EN, CHATBOT_COMMENT_CODE, CHANNEL, RESPONSE_TIME, REG_DATE, ACTIVE_FLAG, APP_ID) ";
                cmd.CommandText += " VALUES ";
                cmd.CommandText += " (@userNumber, @customerCommentKR, @customerCommentEN, @chatbotCommentCode, @channel, @responseTime, CONVERT(VARCHAR,  GETDATE(), 101) + ' ' + CONVERT(VARCHAR,  GETDATE(), 24), 0, @appID) ";

                cmd.Parameters.AddWithValue("@userNumber", userNumber);
                cmd.Parameters.AddWithValue("@customerCommentKR", customerCommentKR);
                cmd.Parameters.AddWithValue("@customerCommentEN", customerCommentEN);
                cmd.Parameters.AddWithValue("@chatbotCommentCode", chatbotCommentCode);
                cmd.Parameters.AddWithValue("@channel", channel);
                cmd.Parameters.AddWithValue("@responseTime", responseTime);
                cmd.Parameters.AddWithValue("@appID", appID);

                result = cmd.ExecuteNonQuery();
                Debug.WriteLine("result : " + result);
            }
            return result;
        }

       

        //견적 쿼리

        public List<CarModelList> SelectCarModelList()
        {
            SqlDataReader rdr = null;
            List<CarModelList> carModel = new List<CarModelList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                //cmd.CommandText = "SELECT DLG_ID, DLG_NM, DLG_MENT, DLG_LANG FROM TBL_DLG WHERE DLG_ID = '" + dlgID  + "' AND USE_YN = 'Y'";
                cmd.CommandText = " SELECT CAR_CD_TYPE "
                                  + " ,SUBSTRING(CAR_NAME, CHARINDEX(' ', CAR_NAME) + 1, LEN(CAR_NAME)) AS MODEL_NM "
                                  + " FROM TBL_CARNM ";

                //cmd.Parameters.AddWithValue("@dlgID", dlgID);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    //int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string carCdType = rdr["CAR_CD_TYPE"] as string;
                    string carModelNm = rdr["MODEL_NM"] as string;


                    CarModelList model = new CarModelList();
                    model.carCdType = carCdType;
                    model.carModelNm = carModelNm;

                    carModel.Add(model);
                }
            }
            return carModel;
        }

        public List<CarTrimList> SelectCarTrimList(string modelNm)
        {
            SqlDataReader rdr = null;
            List<CarTrimList> carTrimList = new List<CarTrimList>();

            Debug.WriteLine("SelectCarTrimList1 : ");

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += " select CAR_TRIM_NM  ";
                cmd.CommandText += " 			,SALE_CD ";
                cmd.CommandText += " 			,FNUM_CD ";
                cmd.CommandText += " 			,SALE_PRICE  ";
                cmd.CommandText += " 			,FUEL ";
                cmd.CommandText += " 			,DRIVEWHEEL ";
                cmd.CommandText += " 			,TUIX ";
                cmd.CommandText += " 			,COLORPACKAGE ";
                cmd.CommandText += " 			,CARTRIM ";
                cmd.CommandText += " 	from FN_PRICE_TRIM ('" + modelNm + "')  ";
                cmd.CommandText += " 	ORDER BY CAR_TRIM_NM    ";

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string carTrimNm = rdr["CAR_TRIM_NM"] as string;
                    string saleCD = rdr["SALE_CD"] as string;
                    string fnumCD = rdr["FNUM_CD"] as string;
                    string fuel = rdr["FUEL"] as string;
                    string driveWheel = rdr["DRIVEWHEEL"] as string;
                    string tuix = rdr["TUIX"] as string;
                    string trimPackage = rdr["COLORPACKAGE"] as string;
                    string carTrim = rdr["CARTRIM"] as string;
                    int salePrice = Convert.ToInt32(rdr["SALE_PRICE"]);

                    CarTrimList trim = new CarTrimList();
                    trim.carTrimNm = carTrimNm;
                    trim.saleCD = saleCD;
                    trim.fnumCD = fnumCD;
                    trim.fuel = fuel;
                    trim.drivewheel = driveWheel;
                    trim.tuix = tuix;
                    trim.trimpackage = trimPackage;
                    trim.cartrim = carTrim;
                    trim.salePrice = salePrice;

                    carTrimList.Add(trim);
                }
            }
            return carTrimList;
        }

        public int SelectFBCarTrimListCnt(string modelNm)
        {
            SqlDataReader rdr = null;
            int cardCnt = 0;

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT COUNT(*) CNT from FN_PRICE_TRIM ('" + modelNm + "')  WHERE SALE_CD NOT LIKE '%XX%'";

                //cmd.Parameters.AddWithValue("@dlgID", modelNm);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    cardCnt = Convert.ToInt32(rdr["CNT"]);
                }
            }
            return cardCnt;
        }

        public List<CarTrimList> SelectFBCarTrimList(string modelNm, int cnt)
        {
            SqlDataReader rdr = null;
            List<CarTrimList> carTrimList = new List<CarTrimList>();

            Debug.WriteLine("SelectCarTrimList1 : ");

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += " select CAR_TRIM_NM  ";
                cmd.CommandText += " 			,SALE_CD ";
                cmd.CommandText += " 			,FNUM_CD ";
                cmd.CommandText += " 			,SALE_PRICE  ";
                cmd.CommandText += " 			,FUEL ";
                cmd.CommandText += " 			,DRIVEWHEEL ";
                cmd.CommandText += " 			,TUIX ";
                cmd.CommandText += " 			,COLORPACKAGE ";
                cmd.CommandText += " 			,CARTRIM ";
                cmd.CommandText += " 	from FN_PRICE_TRIM ('" + modelNm + "')  ";
                cmd.CommandText += " 	ORDER BY CAR_TRIM_NM    ";
                cmd.CommandText += " 	OFFSET @cnt ROWS    ";
                cmd.CommandText += " 	FETCH NEXT 10 ROWS ONLY    ";

                cmd.Parameters.AddWithValue("@cnt", cnt);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string carTrimNm = rdr["CAR_TRIM_NM"] as string;
                    string saleCD = rdr["SALE_CD"] as string;
                    string fnumCD = rdr["FNUM_CD"] as string;
                    string fuel = rdr["FUEL"] as string;
                    string driveWheel = rdr["DRIVEWHEEL"] as string;
                    string tuix = rdr["TUIX"] as string;
                    string trimPackage = rdr["COLORPACKAGE"] as string;
                    string carTrim = rdr["CARTRIM"] as string;
                    int salePrice = Convert.ToInt32(rdr["SALE_PRICE"]);

                    CarTrimList trim = new CarTrimList();
                    trim.carTrimNm = carTrimNm;
                    trim.saleCD = saleCD;
                    trim.fnumCD = fnumCD;
                    trim.fuel = fuel;
                    trim.drivewheel = driveWheel;
                    trim.tuix = tuix;
                    trim.trimpackage = trimPackage;
                    trim.cartrim = carTrim;
                    trim.salePrice = salePrice;

                    carTrimList.Add(trim);
                }
            }
            return carTrimList;
        }



        public int SelectFBCarExColorAllListCnt()
        {
            SqlDataReader rdr = null;
            int cardCnt = 0;

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += " SELECT COUNT(*) CNT FROM ( ";
                cmd.CommandText += " SELECT 	";
                cmd.CommandText += "  	  TRIMCOLOR_NM, TRIMCOLOR_CD, TRIMCOLOR_PRICE	";
                cmd.CommandText += "   FROM TBL_TRIMCOLOR2	";
                cmd.CommandText += "   WHERE TRIMCOLOR_NM NOT LIKE '%)'	";
                cmd.CommandText += "   GROUP BY TRIMCOLOR_NM, TRIMCOLOR_CD, TRIMCOLOR_PRICE	) A";

                //cmd.Parameters.AddWithValue("@dlgID", modelNm);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    cardCnt = Convert.ToInt32(rdr["CNT"]);
                }
            }
            return cardCnt;
        }

        public List<CarExColorList> SelectCarExColorAllList()
        {
            SqlDataReader rdr = null;
            List<CarExColorList> carExColorList = new List<CarExColorList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += " SELECT TRIMCOLOR_NM	";
                cmd.CommandText += "       ,LEFT(TRIMCOLOR_CD,CHARINDEX(':',TRIMCOLOR_CD)-1) AS TRIMCOLOR_CD	";
                cmd.CommandText += "  	  ,TRIMCOLOR_PRICE	";
                cmd.CommandText += "   FROM TBL_TRIMCOLOR2	";
                cmd.CommandText += "   WHERE TRIMCOLOR_NM NOT LIKE '%)'	";
                cmd.CommandText += "   GROUP BY TRIMCOLOR_NM, TRIMCOLOR_CD, TRIMCOLOR_PRICE	";

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string trimColorNm = rdr["TRIMCOLOR_NM"] as string;
                    string trimColorCd = rdr["TRIMCOLOR_CD"] as string;
                    int exColorPrice = Convert.ToInt32(rdr["TRIMCOLOR_PRICE"]);
                    //string model = rdr["MODEL_NAME"] as string;

                    CarExColorList exColor = new CarExColorList();
                    exColor.trimColorNm = trimColorNm;
                    exColor.trimColorCd = trimColorCd;
                    exColor.exColorPrice = exColorPrice;
                    //exColor.model = model;

                    carExColorList.Add(exColor);
                }
            }
            return carExColorList;
        }


        public List<CarExColorList> SelectFBCarExColorAllList(int cnt)
        {
            SqlDataReader rdr = null;
            List<CarExColorList> carExColorList = new List<CarExColorList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += " SELECT TRIMCOLOR_NM	";
                cmd.CommandText += "       ,LEFT(TRIMCOLOR_CD,CHARINDEX(':',TRIMCOLOR_CD)-1) AS TRIMCOLOR_CD	";
                cmd.CommandText += "  	  ,TRIMCOLOR_PRICE	";
                cmd.CommandText += "   FROM TBL_TRIMCOLOR2	";
                cmd.CommandText += "   WHERE TRIMCOLOR_NM NOT LIKE '%)'	";
                cmd.CommandText += "   GROUP BY TRIMCOLOR_NM, TRIMCOLOR_CD, TRIMCOLOR_PRICE	";
                cmd.CommandText += "   ORDER BY TRIMCOLOR_NM, TRIMCOLOR_CD, TRIMCOLOR_PRICE ";
                cmd.CommandText += "   OFFSET @cnt ROWS ";
                cmd.CommandText += "   FETCH NEXT 10 ROWS ONLY ";

                cmd.Parameters.AddWithValue("@cnt", cnt);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string trimColorNm = rdr["TRIMCOLOR_NM"] as string;
                    string trimColorCd = rdr["TRIMCOLOR_CD"] as string;
                    int exColorPrice = Convert.ToInt32(rdr["TRIMCOLOR_PRICE"]);
                    //string model = rdr["MODEL_NAME"] as string;

                    CarExColorList exColor = new CarExColorList();
                    exColor.trimColorNm = trimColorNm;
                    exColor.trimColorCd = trimColorCd;
                    exColor.exColorPrice = exColorPrice;
                    //exColor.model = model;

                    carExColorList.Add(exColor);
                }
            }
            return carExColorList;
        }


        public List<CarExColorList> SelectCarExColorSelectList(string color)
        {
            SqlDataReader rdr = null;
            List<CarExColorList> carExColorList = new List<CarExColorList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += " SELECT TRIMCOLOR_NM	";
                cmd.CommandText += "       ,LEFT(TRIMCOLOR_CD,CHARINDEX(':',TRIMCOLOR_CD)-1) AS TRIMCOLOR_CD	";
                cmd.CommandText += "  	  ,TRIMCOLOR_PRICE	";
                cmd.CommandText += "   FROM TBL_TRIMCOLOR2	";
                cmd.CommandText += "   WHERE TRIMCOLOR_NM NOT LIKE '%)'	";
                cmd.CommandText += "   AND TRIMCOLOR_CD LIKE '%"+ color + "%'	";
                cmd.CommandText += "   GROUP BY TRIMCOLOR_NM, TRIMCOLOR_CD, TRIMCOLOR_PRICE	";

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string trimColorNm = rdr["TRIMCOLOR_NM"] as string;
                    string trimColorCd = rdr["TRIMCOLOR_CD"] as string;
                    int exColorPrice = Convert.ToInt32(rdr["TRIMCOLOR_PRICE"]);
                    //string model = rdr["MODEL_NAME"] as string;

                    CarExColorList exColor = new CarExColorList();
                    exColor.trimColorNm = trimColorNm;
                    exColor.trimColorCd = trimColorCd;
                    exColor.exColorPrice = exColorPrice;
                    //exColor.model = model;

                    carExColorList.Add(exColor);
                }
            }
            return carExColorList;
        }

        public int SelectFBCarInColorAllListCnt()
        {
            SqlDataReader rdr = null;
            int cardCnt = 0;

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += " SELECT COUNT(*) CNT FROM ( ";
                cmd.CommandText += " SELECT 				";
                cmd.CommandText += "  	 INTERNALCOLOR_NM , INTERNALCOLOR_CD  , INTERNALCOLOR_PRICE				";
                cmd.CommandText += " FROM TBL_INTERCOLOR2 B				";
                cmd.CommandText += " GROUP BY INTERNALCOLOR_NM , INTERNALCOLOR_CD  , INTERNALCOLOR_PRICE ) A	";

                //cmd.Parameters.AddWithValue("@dlgID", modelNm);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    cardCnt = Convert.ToInt32(rdr["CNT"]);
                }
            }
            return cardCnt;
        }

        public List<CarInColorList> SelectCarInColorAllList()
        {
            SqlDataReader rdr = null;
            List<CarInColorList> carInColorList = new List<CarInColorList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += " SELECT INTERNALCOLOR_NM				";
                cmd.CommandText += "       , INTERNALCOLOR_CD				";
                cmd.CommandText += "  	 , INTERNALCOLOR_PRICE				";
                cmd.CommandText += " FROM TBL_INTERCOLOR2 B				";
                cmd.CommandText += " GROUP BY INTERNALCOLOR_NM , INTERNALCOLOR_CD  , INTERNALCOLOR_PRICE				";

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string inColorNm = rdr["INTERNALCOLOR_NM"] as string;
                    string inColorCd = rdr["INTERNALCOLOR_CD"] as string;
                    int inColorPrice = Convert.ToInt32(rdr["INTERNALCOLOR_PRICE"]);
                    //string model = rdr["MODEL_NAME"] as string;

                    CarInColorList inColor = new CarInColorList();
                    inColor.internalColorNm = inColorNm;
                    inColor.internalColorCd = inColorCd;
                    inColor.inColorPrice = inColorPrice;
                    //inColor.model = model;
                    //exColor.model = model;

                    carInColorList.Add(inColor);
                }
            }
            return carInColorList;
        }


        public List<CarInColorList> SelectFBCarInColorAllList(int cnt)
        {
            SqlDataReader rdr = null;
            List<CarInColorList> carInColorList = new List<CarInColorList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += " SELECT INTERNALCOLOR_NM				";
                cmd.CommandText += "       , INTERNALCOLOR_CD				";
                cmd.CommandText += "  	 , INTERNALCOLOR_PRICE				";
                cmd.CommandText += " FROM TBL_INTERCOLOR2 B				";
                cmd.CommandText += " GROUP BY INTERNALCOLOR_NM , INTERNALCOLOR_CD  , INTERNALCOLOR_PRICE				";
                cmd.CommandText += " ORDER BY INTERNALCOLOR_NM , INTERNALCOLOR_CD  , INTERNALCOLOR_PRICE				";
                cmd.CommandText += "   OFFSET @cnt ROWS ";
                cmd.CommandText += "   FETCH NEXT 10 ROWS ONLY ";

                cmd.Parameters.AddWithValue("@cnt", cnt);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string inColorNm = rdr["INTERNALCOLOR_NM"] as string;
                    string inColorCd = rdr["INTERNALCOLOR_CD"] as string;
                    int inColorPrice = Convert.ToInt32(rdr["INTERNALCOLOR_PRICE"]);
                    //string model = rdr["MODEL_NAME"] as string;

                    CarInColorList inColor = new CarInColorList();
                    inColor.internalColorNm = inColorNm;
                    inColor.internalColorCd = inColorCd;
                    inColor.inColorPrice = inColorPrice;
                    //inColor.model = model;
                    //exColor.model = model;

                    carInColorList.Add(inColor);
                }
            }
            return carInColorList;
        }




        public List<CarExColorList> SelectCarExColorList(string modelNm)
        {
            SqlDataReader rdr = null;
            List<CarExColorList> carExColorList = new List<CarExColorList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += " SELECT TRIMCOLOR_NM											";
                cmd.CommandText += "      , LEFT(TRIMCOLOR_CD,CHARINDEX(':',TRIMCOLOR_CD)-1) AS TRIMCOLOR_CD                                            ";
                cmd.CommandText += " 	 , TRIMCOLOR_PRICE                                          ";
                cmd.CommandText += " 	 , MODEL_NAME                                               ";
                cmd.CommandText += "  FROM TBL_TRIMCOLOR2  A                                        ";
                cmd.CommandText += "  JOIN                                                          ";
                cmd.CommandText += "  (                                                             ";
                cmd.CommandText += " 	SELECT B.SALE_CD, B.FNUM_CD, B.MODEL_NAME                   ";
                cmd.CommandText += " 	FROM TBL_CAR_TRIM_DEF B                                     ";
                cmd.CommandText += " 	WHERE MODEL_NAME = @modelNm                                 ";
                cmd.CommandText += "  ) AA                                                          ";
                cmd.CommandText += " ON A.SALE_CD = AA.SALE_CD AND A.MINUS_OPT = AA.FNUM_CD         ";

                cmd.Parameters.AddWithValue("@modelNm", modelNm);

                Debug.WriteLine("query : " + cmd);


                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string trimColorNm = rdr["TRIMCOLOR_NM"] as string;
                    string trimColorCd = rdr["TRIMCOLOR_CD"] as string;
                    int exColorPrice = Convert.ToInt32(rdr["TRIMCOLOR_PRICE"]);
                    string model = rdr["MODEL_NAME"] as string;

                    CarExColorList exColor = new CarExColorList();
                    exColor.trimColorNm = trimColorNm;
                    exColor.trimColorCd = trimColorCd;
                    exColor.exColorPrice = exColorPrice;
                    exColor.model = model;

                    carExColorList.Add(exColor);
                }
            }
            return carExColorList;
        }


        public List<CarInColorList> SelectCarInColorList(string modelNm)
        {
            SqlDataReader rdr = null;
            List<CarInColorList> carInColorList = new List<CarInColorList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += " SELECT INTERNALCOLOR_NM                                                                          ";
                cmd.CommandText += "      , INTERNALCOLOR_CD                                                                          ";
                cmd.CommandText += " 	 , INTERNALCOLOR_PRICE                                                                        ";
                cmd.CommandText += " 	 , MODEL_NAME                                                                                 ";
                cmd.CommandText += "  FROM TBL_INTERCOLOR2 B JOIN                                                                     ";
                cmd.CommandText += "  (                                                                                               ";
                cmd.CommandText += " 	SELECT	A.SALE_CD,                                                                            ";
                cmd.CommandText += " 			A.MINUS_OPT,                                                                          ";
                cmd.CommandText += " 			AA.MODEL_NAME                                                                         ";
                cmd.CommandText += " 	 FROM TBL_TRIMCOLOR2  A JOIN                                                                  ";
                cmd.CommandText += " 	 (                                                                                            ";
                cmd.CommandText += " 		SELECT B.SALE_CD, B.FNUM_CD  , B.MODEL_NAME                                               ";
                cmd.CommandText += " 		FROM TBL_CAR_TRIM_DEF B                                                                   ";
                cmd.CommandText += " 		WHERE MODEL_NAME = @modelNm                                 							  ";
                cmd.CommandText += " 	 ) AA                                                                                         ";
                cmd.CommandText += " 	ON A.SALE_CD = AA.SALE_CD AND A.MINUS_OPT = AA.FNUM_CD                                        ";
                cmd.CommandText += "  )BB                                                                                             ";
                cmd.CommandText += "  ON B.SALE_CD = BB.SALE_CD AND B.MINUS_OPT = BB.MINUS_OPT                                        ";
                cmd.CommandText += "  GROUP BY INTERNALCOLOR_NM , INTERNALCOLOR_CD  , INTERNALCOLOR_PRICE , MODEL_NAME                ";

                cmd.Parameters.AddWithValue("@modelNm", modelNm);

                Debug.WriteLine("query : " + cmd);


                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string inColorNm = rdr["INTERNALCOLOR_NM"] as string;
                    string inColorCd = rdr["INTERNALCOLOR_CD"] as string;
                    int inColorPrice = Convert.ToInt32(rdr["INTERNALCOLOR_PRICE"]);
                    string model = rdr["MODEL_NAME"] as string;

                    CarInColorList inColor = new CarInColorList();
                    inColor.internalColorNm = inColorNm;
                    inColor.internalColorCd = inColorCd;
                    inColor.inColorPrice = inColorPrice;
                    inColor.model = model;

                    carInColorList.Add(inColor);
                }
            }
            return carInColorList;
        }



        public int SelectFBOptionListCnt(string msg)
        {
            SqlDataReader rdr = null;
            int cardCnt = 0;

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT COUNT(*) CNT FROM FN_PRICE_OPTION ('" + msg + "')  ";

                //cmd.Parameters.AddWithValue("@dlgID", modelNm);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    cardCnt = Convert.ToInt32(rdr["CNT"]);
                }
            }
            return cardCnt;
        }

        public List<CarOptionList> SelectOptionList(string msg)
        {
            SqlDataReader rdr = null;
            List<CarOptionList> carOptionList = new List<CarOptionList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "  SELECT OPT_NM                                         ";
                cmd.CommandText += "      , OPT_PRICE ,OPT_CD       , PKG_DT                ";
                cmd.CommandText += "  FROM FN_PRICE_OPTION                                  ";
                cmd.CommandText += "  ('" + msg + "')                                       ";
                cmd.CommandText += "  ORDER BY OPT_NM                                       ";

                //cmd.Parameters.AddWithValue("@msg", msg);


                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string optNm = rdr["OPT_NM"] as string;
                    int optPrice = Convert.ToInt32(rdr["OPT_PRICE"]);
                    //string model = rdr["MODEL_NAME"] as string;
                    string optCd = rdr["OPT_CD"] as string;
                    string pkgDt = rdr["PKG_DT"] as string;


                    CarOptionList option = new CarOptionList();
                    option.optNm = optNm;
                    option.optPrice = optPrice;
                    //option.model = model;
                    option.optCd = optCd;
                    option.pkgDt = pkgDt;
                    carOptionList.Add(option);
                }
            }
            return carOptionList;
        }

        public List<CarOptionList> SelectFBOptionList(string msg , int cnt)
        {
            SqlDataReader rdr = null;
            List<CarOptionList> carOptionList = new List<CarOptionList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "  SELECT OPT_NM                                         ";
                cmd.CommandText += "      , OPT_PRICE ,OPT_CD       , PKG_DT                ";
                cmd.CommandText += "  FROM FN_PRICE_OPTION                                  ";
                cmd.CommandText += "  ('" + msg + "')                                       ";
                cmd.CommandText += "  ORDER BY OPT_NM                                       ";
                cmd.CommandText += "   OFFSET @cnt ROWS ";
                cmd.CommandText += "   FETCH NEXT 10 ROWS ONLY ";

                cmd.Parameters.AddWithValue("@cnt", cnt);


                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string optNm = rdr["OPT_NM"] as string;
                    int optPrice = Convert.ToInt32(rdr["OPT_PRICE"]);
                    //string model = rdr["MODEL_NAME"] as string;
                    string optCd = rdr["OPT_CD"] as string;
                    string pkgDt = rdr["PKG_DT"] as string;


                    CarOptionList option = new CarOptionList();
                    option.optNm = optNm;
                    option.optPrice = optPrice;
                    //option.model = model;
                    option.optCd = optCd;
                    option.pkgDt = pkgDt;
                    carOptionList.Add(option);
                }
            }
            return carOptionList;
        }



        //DB
        public List<CarOptionList> SelectCarOptionList(string modelNm)
        {
            SqlDataReader rdr = null;
            List<CarOptionList> carOptionList = new List<CarOptionList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "  SELECT OPT_NM                                                                                   ";
                cmd.CommandText += "      , OPT_PRICE ,OPT_CD       , MODEL_NAME                                                      ";
                cmd.CommandText += "  FROM TBL_OPT A JOIN                                                                             ";
                cmd.CommandText += "  (                                                                                               ";
                cmd.CommandText += " 	SELECT B.SALE_CD, B.FNUM_CD , B.MODEL_NAME                                                    ";
                cmd.CommandText += " 		FROM TBL_CAR_TRIM_DEF B                                                                   ";
                cmd.CommandText += " 		WHERE MODEL_NAME = @modelNm                                 							  ";
                cmd.CommandText += "  ) AA                                                                                            ";
                cmd.CommandText += " ON A.SALE_CD = AA.SALE_CD AND A.FNUM_CD = AA.FNUM_CD                                             ";

                cmd.Parameters.AddWithValue("@modelNm", modelNm);

                Debug.WriteLine("query : " + cmd);


                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string optNm = rdr["OPT_NM"] as string;
                    int optPrice = Convert.ToInt32(rdr["OPT_PRICE"]);
                    string model = rdr["MODEL_NAME"] as string;
                    string optCd = rdr["OPT_CD"] as string;


                    CarOptionList option = new CarOptionList();
                    option.optNm = optNm;
                    option.optPrice = optPrice;
                    option.model = model;
                    option.optCd = optCd;
                    carOptionList.Add(option);
                }
            }
            return carOptionList;
        }

        public string SelectOnlyTrimColor(string arg)
        {
            SqlDataReader rdr = null;
            string color = ""; string color1 = ""; string color2 = "";

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += " SELECT 																";
                cmd.CommandText += " 					A.TUIX,A.CARTRIM                                    ";
                cmd.CommandText += " 				FROM TBL_CARMODEL A                                     ";
                cmd.CommandText += " 				JOIN                                                    ";
                cmd.CommandText += " 				(                                                       ";
                cmd.CommandText += " 					SELECT B.SALE_CD, B.FNUM_CD                         ";
                cmd.CommandText += " 					  FROM TBL_CAR_TRIM_DEF B                           ";
                cmd.CommandText += " 					  WHERE MODEL_NAME = '" + arg + "'                   ";
                cmd.CommandText += " 				) AA                                                    ";
                cmd.CommandText += " 				ON A.SALE_CD = AA.SALE_CD AND A.FNUM_CD = AA.FNUM_CD    ";

                //cmd.Parameters.AddWithValue("@arg", arg);

                Debug.WriteLine("query : " + cmd.CommandText);


                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    color1 = rdr["TUIX"] as string;
                    color2 = rdr["CARTRIM"] as string;
                    //Debug.WriteLine("color1 : " + color1);
                    //Debug.WriteLine("color2 : " + color2);

                }

                color = (string)(color1 + color2);
                //Debug.WriteLine("COLOR : " + color.Replace(" ",""));
            }
            return color;
        }


        public List<CarQouteLuisResult> SelectCarQouteLuisResult(string json)
        {
            SqlDataReader rdr = null;
            List<CarQouteLuisResult> CarQouteLuisResult = new List<CarQouteLuisResult>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                json = json.Replace("'", "''");
                cmd.CommandText += "  select INTENT, ENTITY,ENTITY_VALUE  FROM FN_LUIS_RESULT_PRICE    ";
                cmd.CommandText += "  ('" + json + "')                      ";

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string intentValue = rdr["INTENT"] as string;
                    string entityValue = rdr["ENTITY"] as string;
                    string whereValue = rdr["ENTITY_VALUE"] as string;


                    CarQouteLuisResult result = new CarQouteLuisResult();
                    result.intentValue = intentValue;
                    result.entityValue = entityValue;
                    result.whereValue = whereValue;

                    CarQouteLuisResult.Add(result);
                }
            }
            return CarQouteLuisResult;
        }

        public List<CarQouteLuisResult> SelectLuisResult(string json)
        {
            SqlDataReader rdr = null;
            List<CarQouteLuisResult> CarQouteLuisResult = new List<CarQouteLuisResult>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                json = json.Replace("'", "''");
                cmd.CommandText += "  select INTENT, ENTITY,ENTITY_VALUE  FROM FN_LUIS_RESULT    ";
                cmd.CommandText += "  ('" + json + "')                      ";

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string intentValue = rdr["INTENT"] as string;
                    string entityValue = rdr["ENTITY"] as string;
                    string whereValue = rdr["ENTITY_VALUE"] as string;


                    CarQouteLuisResult result = new CarQouteLuisResult();
                    result.intentValue = intentValue;
                    result.entityValue = entityValue;
                    result.whereValue = whereValue;

                    CarQouteLuisResult.Add(result);
                }
            }
            return CarQouteLuisResult;
        }

        public List<LuisResult> LuisResult(string json)
        {
            SqlDataReader rdr = null;
            List<LuisResult> LuisResult = new List<LuisResult>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                json = json.Replace("'", "''");
                cmd.CommandText += "  SELECT  TOP 1 VAL, INTENT, ENTITY,INTENTS_SCORE, ENTITY_VALUE , TEST_DRIVEWHERE, CAR_PRICEWHERE  FROM FN_LUIS_RESULT_SUM";
                cmd.CommandText += "  ('" + json + "')                      ";

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string val = rdr["VAL"] as string;
                    string intentValue = rdr["INTENT"] as string;
                    string entityValue = rdr["ENTITY"] as string;
                    string intentScore = rdr["INTENTS_SCORE"] as string;
                    string entitieWhereValue = rdr["ENTITY_VALUE"] as string;
                    string testDriveWhereValue = rdr["TEST_DRIVEWHERE"] as string;
                    string carPriceWhereValue = rdr["CAR_PRICEWHERE"] as string;


                    LuisResult result = new LuisResult();
                    result.val = val;
                    result.intentValue = intentValue;
                    result.entityValue = entityValue;
                    result.intentScore = intentScore;
                    result.entitieWhereValue = entitieWhereValue;
                    result.testDriveWhereValue = testDriveWhereValue;
                    result.carPriceWhereValue = carPriceWhereValue;

                    LuisResult.Add(result);
                }
            }
            return LuisResult;
        }



        public string SelectKoreanCashCheck(string arg, int appID)
        {
            SqlDataReader rdr = null;
            string result = ""; 

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "SELECT KR_QUERY FROM TBL_QUERY_ANALYSIS_RESULT WHERE KR_QUERY = '" + arg + "' AND APP_ID = "+appID ;

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    result = rdr["KR_QUERY"] as string;
                }
            }
            return result;
        }

        public string SelectKoreanCashCheck1(string arg, int appID)
        {
            SqlDataReader rdr = null;
            string result = "";

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "SELECT QUERY FROM TBL_QUERY_ANALYSIS_RESULT WHERE KR_QUERY = '" + arg + "' AND APP_ID = " + appID;

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    result = rdr["QUERY"] as string;
                }
            }
            return result;
        }


        public string SelectKoreanCashIntentEntities(string arg, int appID)
        {
            SqlDataReader rdr = null;
            string result = "";

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "SELECT INTENT_ID, ENTITIES_IDS FROM TBL_QUERY_ANALYSIS_RESULT WHERE KR_QUERY = '" + arg + "' AND APP_ID = " + appID; ;

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    result = rdr["INTENT_ID"] as string +"@@"+ rdr["ENTITIES_IDS"] as string;
                }
            }
            return result;
        }




        public string SelectEnglishCashCheck(string arg, int appID)
        {
            SqlDataReader rdr = null;
            string result = "";
            arg = arg.Replace("'", "''");
            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "SELECT QUERY FROM TBL_QUERY_ANALYSIS_RESULT WHERE QUERY = '" + arg + "' AND APP_ID = " + appID; ;

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    result = rdr["QUERY"] as string;
                }
            }
            return result;
        }




        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Query Analysis
        // Check if dialogue already exists, return Luis format JSON.
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public JObject SelectQueryAnalysis(string query, int appID)
        {
            String json = @"{
                'entities':'',
                'intents':[{'intent':''}],
                'intent_score':'',
                'test_driveWhere':'',
                'car_priceWhere':'',
                'car_option':''
            }";
            JObject returnJson = JObject.Parse(json);

            SqlDataReader rdr = null;
            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                //cmd.CommandText = "SELECT TOP 1 INTENT_ID, ENTITIES_IDS, ISNULL(CAR_COLOR,'') AS CAR_COLOR, ISNULL(CAR_AREA,'') AS CAR_AREA  ,ISNULL(CAR_PRICEWHERE,'') AS CAR_PRICEWHERE  ,ISNULL(CAR_OPTION,'') AS CAR_OPTION FROM TBL_QUERY_ANALYSIS_RESULT2 WHERE QUERY = @query AND RESULT = 'H'";
                cmd.CommandText = "SELECT TOP 1 INTENT_ID, ENTITIES_IDS, ISNULL(INTENT_SCORE,'') AS INTENT_SCORE, ISNULL(CAR_COLOR,'') AS CAR_COLOR, ISNULL(CAR_AREA,'') AS CAR_AREA  ,ISNULL(CAR_PRICEWHERE,'') AS CAR_PRICEWHERE  ,ISNULL(CAR_OPTION,'') AS CAR_OPTION FROM TBL_QUERY_ANALYSIS_RESULT WHERE QUERY = @query AND RESULT = 'H' AND APP_ID = @appID";
                cmd.Parameters.AddWithValue("@query", query.Trim().ToLower());
                cmd.Parameters.AddWithValue("@appID", appID);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    returnJson["entities"] = rdr["ENTITIES_IDS"] as string;
                    returnJson["intents"][0]["intent"] = rdr["INTENT_ID"] as string;
                    returnJson["intent_score"] = rdr["INTENT_SCORE"] as string;
                    returnJson["test_driveWhere"] = rdr["CAR_AREA"] as string;
                    returnJson["car_priceWhere"] = rdr["CAR_PRICEWHERE"] as string;
                    returnJson["car_option"] = rdr["car_option"] as string;
                }
            }
            return returnJson;
        }



        public JObject SelectQueryAnalysisKor(string query, int appID)
        {
            String json = @"{
                'entities':'',
                'intents':[{'intent':''}],
                'intent_score':'',
                'test_driveWhere':'',
                'car_priceWhere':'',
                'car_option':''
            }";
            JObject returnJson = JObject.Parse(json);

            SqlDataReader rdr = null;
            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                //cmd.CommandText = "SELECT TOP 1 INTENT_ID, ENTITIES_IDS, ISNULL(CAR_COLOR,'') AS CAR_COLOR, ISNULL(CAR_AREA,'') AS CAR_AREA  ,ISNULL(CAR_PRICEWHERE,'') AS CAR_PRICEWHERE  ,ISNULL(CAR_OPTION,'') AS CAR_OPTION FROM TBL_QUERY_ANALYSIS_RESULT2 WHERE QUERY = @query AND RESULT = 'H'";
                cmd.CommandText = "SELECT TOP 1 INTENT_ID, ENTITIES_IDS, ISNULL(INTENT_SCORE,'') AS INTENT_SCORE, ISNULL(CAR_COLOR,'') AS CAR_COLOR, ISNULL(CAR_AREA,'') AS CAR_AREA  ,ISNULL(CAR_PRICEWHERE,'') AS CAR_PRICEWHERE  ,ISNULL(CAR_OPTION,'') AS CAR_OPTION FROM TBL_QUERY_ANALYSIS_RESULT WHERE KR_QUERY = @query AND RESULT = 'H' AND APP_ID = @appID";
                cmd.Parameters.AddWithValue("@query", query.Trim().ToLower());
                cmd.Parameters.AddWithValue("@appID", appID);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    returnJson["entities"] = rdr["ENTITIES_IDS"] as string;
                    returnJson["intents"][0]["intent"] = rdr["INTENT_ID"] as string;
                    returnJson["intent_score"] = rdr["INTENT_SCORE"] as string;
                    returnJson["test_driveWhere"] = rdr["CAR_AREA"] as string;
                    returnJson["car_priceWhere"] = rdr["CAR_PRICEWHERE"] as string;
                    returnJson["car_option"] = rdr["car_option"] as string;
                }
            }
            return returnJson;
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Query Analysis
        // Insert user chat message for history and analysis
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public int insertUserQuery(string korQuery, string enQuery, string intentID, string entitiesIDS, string intentScore,int luisID, char result, string car_area, string car_colorArea, string car_priceWhere, string car_option, int appID)
        {
            int dbResult = 0;
            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "sp_insertusehistory4";

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@krQuery", korQuery.Trim().ToLower());
                cmd.Parameters.AddWithValue("@query", enQuery.Trim().ToLower());
                cmd.Parameters.AddWithValue("@intentID", intentID.Trim());
                cmd.Parameters.AddWithValue("@entitiesIDS", entitiesIDS.Trim().ToLower());
                cmd.Parameters.AddWithValue("@intentScore", intentScore.Trim().ToLower());
                cmd.Parameters.AddWithValue("@luisID", luisID);
                cmd.Parameters.AddWithValue("@result", result);
                cmd.Parameters.AddWithValue("@car_color", car_colorArea);
                cmd.Parameters.AddWithValue("@car_area", car_area);
                cmd.Parameters.AddWithValue("@car_priceWhere", car_priceWhere);
                cmd.Parameters.AddWithValue("@car_option", car_option);
                cmd.Parameters.AddWithValue("@appID", appID);


                dbResult = cmd.ExecuteNonQuery();
            }
            return dbResult;
        }

        


        public int SelectFBTestDriveListCnt(string arg)
        {
            SqlDataReader rdr = null;
            int cardCnt = 0;

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT COUNT(*) CNT FROM FN_LUIS_BRANCH_DRIVE (@arg) ";

                cmd.Parameters.AddWithValue("@arg", arg);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    cardCnt = Convert.ToInt32(rdr["CNT"]);
                }
            }
            return cardCnt;
        }

        public List<TestDriveList> SelectTestDriveList(String arg)
        {
            SqlDataReader rdr = null;

            List<TestDriveList> testDriveList = new List<TestDriveList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "    SELECT GUBUN, STR1, STR2, STR3, STR4, STR5, STR6, STR7, STR8 FROM FN_LUIS_BRANCH_DRIVE (@arg) ";

                cmd.Parameters.AddWithValue("@arg", arg);

                Debug.WriteLine("query : " + cmd.CommandText);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    //int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string dlgGubun = rdr["GUBUN"] as string;
                    string dlgStr1 = rdr["STR1"] as string;
                    string dlgStr2 = rdr["STR2"] as string;
                    string dlgStr3 = rdr["STR3"] as string;
                    string dlgStr4 = rdr["STR4"] as string;
                    string dlgStr5 = rdr["STR5"] as string;
                    string dlgStr6 = rdr["STR6"] as string;
                    string dlgStr7 = rdr["STR7"] as string;
                    string dlgStr8 = rdr["STR8"] as string;


                    TestDriveList dlg = new TestDriveList();
                    dlg.dlgGubun = dlgGubun;
                    dlg.dlgStr1 = dlgStr1;
                    dlg.dlgStr2 = dlgStr2;
                    dlg.dlgStr3 = dlgStr3;
                    dlg.dlgStr4 = dlgStr4;
                    dlg.dlgStr5 = dlgStr5;
                    dlg.dlgStr6 = dlgStr6;
                    dlg.dlgStr7 = dlgStr7;
                    dlg.dlgStr8 = dlgStr8;

                    testDriveList.Add(dlg);
                }
            }
            return testDriveList;
        }

        public List<TestDriveList> SelectFBTestDriveList(String arg, int cnt)
        {
            SqlDataReader rdr = null;

            List<TestDriveList> testDriveList = new List<TestDriveList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "	SELECT GUBUN, STR1, STR2, STR3, STR4, STR5, STR6, STR7, STR8 ";
                cmd.CommandText += "	FROM(   ";
                cmd.CommandText += "	  SELECT ROW_NUMBER() OVER( ORDER BY GUBUN DESC) AS NUM, GUBUN, STR1, STR2, STR3, STR4, STR5, STR6, STR7, STR8 ";
                cmd.CommandText += "	  FROM FN_LUIS_BRANCH_DRIVE (@arg)  ) A ";
                cmd.CommandText += "	ORDER BY NUM    ";
                cmd.CommandText += " 	OFFSET @cnt ROWS    ";
                cmd.CommandText += " 	FETCH NEXT 10 ROWS ONLY    ";

                cmd.Parameters.AddWithValue("@arg", arg);
                cmd.Parameters.AddWithValue("@cnt", cnt);

                Debug.WriteLine("query : " + cmd.CommandText);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    //int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string dlgGubun = rdr["GUBUN"] as string;
                    string dlgStr1 = rdr["STR1"] as string;
                    string dlgStr2 = rdr["STR2"] as string;
                    string dlgStr3 = rdr["STR3"] as string;
                    string dlgStr4 = rdr["STR4"] as string;
                    string dlgStr5 = rdr["STR5"] as string;
                    string dlgStr6 = rdr["STR6"] as string;
                    string dlgStr7 = rdr["STR7"] as string;
                    string dlgStr8 = rdr["STR8"] as string;


                    TestDriveList dlg = new TestDriveList();
                    dlg.dlgGubun = dlgGubun;
                    dlg.dlgStr1 = dlgStr1;
                    dlg.dlgStr2 = dlgStr2;
                    dlg.dlgStr3 = dlgStr3;
                    dlg.dlgStr4 = dlgStr4;
                    dlg.dlgStr5 = dlgStr5;
                    dlg.dlgStr6 = dlgStr6;
                    dlg.dlgStr7 = dlgStr7;
                    dlg.dlgStr8 = dlgStr8;

                    testDriveList.Add(dlg);
                }
            }
            return testDriveList;
        }



        public List<TestDriveListInit> SelectTestDriveListInit(String arg)
        {
            SqlDataReader rdr = null;

            List<TestDriveListInit> testDriveList = new List<TestDriveListInit>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "    SELECT GUBUN, STR1, STR2, STR3, STR4, STR5, STR6, STR7, STR8 FROM FN_LUIS_BRANCH_DRIVE (@arg) ";

                cmd.Parameters.AddWithValue("@arg", arg);

                Debug.WriteLine("query : " + cmd.CommandText);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    //int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string dlgGubun = rdr["GUBUN"] as string;
                    string dlgStr1 = rdr["STR1"] as string;
                    string dlgStr2 = rdr["STR2"] as string;
                    string dlgStr3 = rdr["STR3"] as string;
                    string dlgStr4 = rdr["STR4"] as string;
                    string dlgStr5 = rdr["STR5"] as string;
                    string dlgStr6 = rdr["STR6"] as string;
                    string dlgStr7 = rdr["STR7"] as string;
                    string dlgStr8 = rdr["STR8"] as string;


                    TestDriveListInit dlg = new TestDriveListInit();
                    dlg.dlgGubun = dlgGubun;
                    dlg.dlgStr1 = dlgStr1;
                    dlg.dlgStr2 = dlgStr2;
                    dlg.dlgStr3 = dlgStr3;
                    dlg.dlgStr4 = dlgStr4;
                    dlg.dlgStr5 = dlgStr5;
                    dlg.dlgStr6 = dlgStr6;
                    dlg.dlgStr7 = dlgStr7;
                    dlg.dlgStr8 = dlgStr8;

                    testDriveList.Add(dlg);
                }
            }
            return testDriveList;
        }

        public string LocationValue(string arg1, string arg2)
        {
            SqlDataReader rdr = null;
            string result = "";
            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "SELECT TOP 1 (SELECT CHG_WORD FROM TBL_WORD_CHG_DICT WHERE ORG_WORD = BR_DTL_ADDR1) AS BR_DTL_ADDR1 ";
                cmd.CommandText += "FROM    ";
                cmd.CommandText += "    (   ";
                cmd.CommandText += "        SELECT dbo.FN_TO_DISTANCE(@arg1, @arg2, BR_XCOO, BR_YCOO) AS DISTANCE, BR_DTL_ADDR1    ";
                cmd.CommandText += "        FROM TBL_BR_MGMT ";
                cmd.CommandText += "    ) A ";
                cmd.CommandText += "ORDER BY A.DISTANCE     ";

                cmd.Parameters.AddWithValue("@arg1", arg1);
                cmd.Parameters.AddWithValue("@arg2", arg2);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    result = rdr["BR_DTL_ADDR1"] as string;
                }
            }
            return result;
        }

        public List<RecommendList> SelectRecommendList()
        {
            SqlDataReader rdr = null;
            List<RecommendList> recommendList = new List<RecommendList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "SELECT			                ";
                cmd.CommandText += "TRIM_DETAIL,		            ";
                cmd.CommandText += "ANSWER_1, ANSWER_2, ANSWER_3,   ";
                cmd.CommandText += "OPTION_1, OPTION_1_IMG_URL,	    ";
                cmd.CommandText += "OPTION_2, OPTION_2_IMG_URL,	    ";
                cmd.CommandText += "OPTION_3, OPTION_3_IMG_URL,	    ";
                cmd.CommandText += "OPTION_4, OPTION_4_IMG_URL,	    ";
                cmd.CommandText += "OPTION_5, OPTION_5_IMG_URL,	    ";
                cmd.CommandText += "OPTION_6, OPTION_6_IMG_URL,	    ";
                cmd.CommandText += "MAIN_COLOR_VIEW_1,		        ";
                cmd.CommandText += "MAIN_COLOR_VIEW_2,		        ";
                cmd.CommandText += "MAIN_COLOR_VIEW_3,		        ";
                cmd.CommandText += "MAIN_COLOR_VIEW_4,		        ";
                cmd.CommandText += "MAIN_COLOR_VIEW_5,		        ";
                cmd.CommandText += "MAIN_COLOR_VIEW_6,		        ";
                cmd.CommandText += "MAIN_COLOR_VIEW_7		        ";
                cmd.CommandText += "FROM TBL_RECOMMEND_TRIM	    ";

                /*cmd.Parameters.AddWithValue("@usage", usage);
                cmd.Parameters.AddWithValue("@importance", importance);
                cmd.Parameters.AddWithValue("@genderAge", genderAge);*/

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (rdr.Read())
                {
                    RecommendList dlg = new RecommendList();
                    dlg.TRIM_DETAIL = rdr["TRIM_DETAIL"] as string;
                    dlg.ANSWER_1 = rdr["ANSWER_1"] as string;
                    dlg.ANSWER_2 = rdr["ANSWER_2"] as string;
                    dlg.ANSWER_3 = rdr["ANSWER_3"] as string;
                    dlg.OPTION_1 = rdr["OPTION_1"] as string;
                    dlg.OPTION_1_IMG_URL = rdr["OPTION_1_IMG_URL"] as string;
                    dlg.OPTION_2 = rdr["OPTION_2"] as string;
                    dlg.OPTION_2_IMG_URL = rdr["OPTION_2_IMG_URL"] as string;
                    dlg.OPTION_3 = rdr["OPTION_3"] as string;
                    dlg.OPTION_3_IMG_URL = rdr["OPTION_3_IMG_URL"] as string;
                    dlg.OPTION_4 = rdr["OPTION_4"] as string;
                    dlg.OPTION_4_IMG_URL = rdr["OPTION_4_IMG_URL"] as string;
                    dlg.OPTION_5 = rdr["OPTION_5"] as string;
                    dlg.OPTION_5_IMG_URL = rdr["OPTION_5_IMG_URL"] as string;
                    dlg.OPTION_6 = rdr["OPTION_6"] as string;
                    dlg.OPTION_6_IMG_URL = rdr["OPTION_6_IMG_URL"] as string;
                    dlg.MAIN_COLOR_VIEW_1 = rdr["MAIN_COLOR_VIEW_1"] as string;
                    dlg.MAIN_COLOR_VIEW_2 = rdr["MAIN_COLOR_VIEW_2"] as string;
                    dlg.MAIN_COLOR_VIEW_3 = rdr["MAIN_COLOR_VIEW_3"] as string;
                    dlg.MAIN_COLOR_VIEW_4 = rdr["MAIN_COLOR_VIEW_4"] as string;
                    dlg.MAIN_COLOR_VIEW_5 = rdr["MAIN_COLOR_VIEW_5"] as string;
                    dlg.MAIN_COLOR_VIEW_6 = rdr["MAIN_COLOR_VIEW_6"] as string;
                    dlg.MAIN_COLOR_VIEW_7 = rdr["MAIN_COLOR_VIEW_7"] as string;

                    recommendList.Add(dlg);
                }

            }
            return recommendList;
        }

        public List<RecommendList> SelectedRecommendList(string usage, string importance, string genderAge)
        {
            SqlDataReader rdr = null;
            List<RecommendList> recommendList = new List<RecommendList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "SELECT TOP 1 RECOMMEND_TITLE, ANSWER_1, ANSWER_2, ANSWER_3, TRIM_DETAIL, TRIM_DETAIL_PRICE, ";
                cmd.CommandText += "REPLACE(OPTION_1,'추가','@추가') AS OPTION_1, OPTION_1_IMG_URL, ";
                cmd.CommandText += "REPLACE(OPTION_2,'추가','@추가') AS OPTION_2, OPTION_2_IMG_URL, ";
                cmd.CommandText += "REPLACE(OPTION_3,'추가','@추가') AS OPTION_3, OPTION_3_IMG_URL, ";
                cmd.CommandText += "REPLACE(OPTION_4,'추가','@추가') AS OPTION_4, OPTION_4_IMG_URL, ";
                cmd.CommandText += "OPTION_5, OPTION_5_IMG_URL, ";
                cmd.CommandText += "MAIN_COLOR_VIEW_1, MAIN_COLOR_VIEW_2, MAIN_COLOR_VIEW_3, MAIN_COLOR_VIEW_4, MAIN_COLOR_VIEW_5, MAIN_COLOR_VIEW_6, MAIN_COLOR_VIEW_7, ";
                cmd.CommandText += "REPLACE(MAIN_COLOR_VIEW_NM1,'@@','') AS MAIN_COLOR_VIEW_NM1, ";
                cmd.CommandText += "REPLACE(MAIN_COLOR_VIEW_NM2,'@@','') AS MAIN_COLOR_VIEW_NM2, ";
                cmd.CommandText += "REPLACE(MAIN_COLOR_VIEW_NM3,'@@','') AS MAIN_COLOR_VIEW_NM3, ";
                cmd.CommandText += "REPLACE(MAIN_COLOR_VIEW_NM4,'@@','') AS MAIN_COLOR_VIEW_NM4, ";
                cmd.CommandText += "REPLACE(MAIN_COLOR_VIEW_NM5,'@@','') AS MAIN_COLOR_VIEW_NM5, ";
                cmd.CommandText += "REPLACE(MAIN_COLOR_VIEW_NM6,'@@','') AS MAIN_COLOR_VIEW_NM6, ";
                cmd.CommandText += "REPLACE(MAIN_COLOR_VIEW_NM7,'@@','') AS MAIN_COLOR_VIEW_NM7 ";
                cmd.CommandText += "FROM ";
                cmd.CommandText += "    ( ";
                cmd.CommandText += "    SELECT  RECOMMEND_TITLE, ANSWER_1, ANSWER_2, ANSWER_3, ";
                cmd.CommandText += "             LEFT(TRIM_DETAIL, CHARINDEX('[', TRIM_DETAIL) - 2) AS TRIM_DETAIL, ";
                cmd.CommandText += "               SUBSTRING(TRIM_DETAIL, CHARINDEX('[', TRIM_DETAIL)+1, (CHARINDEX('/', TRIM_DETAIL)) - (CHARINDEX('[', TRIM_DETAIL) + 1)) AS TRIM_DETAIL_PRICE ";
                cmd.CommandText += "              , OPTION_1, OPTION_1_IMG_URL, OPTION_2, OPTION_2_IMG_URL, OPTION_3, OPTION_3_IMG_URL, OPTION_4, OPTION_4_IMG_URL, OPTION_5, OPTION_5_IMG_URL, ";
                cmd.CommandText += "            OPTION_6, OPTION_6_IMG_URL,  ";
                cmd.CommandText += "            (SELECT  LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1) AS TRIMCOLOR_CD FROM TBL_TRIMCOLOR2 WHERE TRIMCOLOR_NM = LEFT(MAIN_COLOR_VIEW_1,CHARINDEX('/',MAIN_COLOR_VIEW_1)-1) GROUP BY LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1)) AS MAIN_COLOR_VIEW_1,  ";
                cmd.CommandText += "            (SELECT  LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1) AS TRIMCOLOR_CD FROM TBL_TRIMCOLOR2 WHERE TRIMCOLOR_NM = LEFT(MAIN_COLOR_VIEW_2,CHARINDEX('/',MAIN_COLOR_VIEW_2)-1) GROUP BY LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1)) AS MAIN_COLOR_VIEW_2,  ";
                cmd.CommandText += "            (SELECT  LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1) AS TRIMCOLOR_CD FROM TBL_TRIMCOLOR2 WHERE TRIMCOLOR_NM = LEFT(MAIN_COLOR_VIEW_3,CHARINDEX('/',MAIN_COLOR_VIEW_3)-1) GROUP BY LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1)) AS MAIN_COLOR_VIEW_3,  ";
                cmd.CommandText += "            (SELECT  LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1) AS TRIMCOLOR_CD FROM TBL_TRIMCOLOR2 WHERE TRIMCOLOR_NM = LEFT(MAIN_COLOR_VIEW_4,CHARINDEX('/',MAIN_COLOR_VIEW_4)-1) GROUP BY LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1)) AS MAIN_COLOR_VIEW_4,  ";
                cmd.CommandText += "            (SELECT  LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1) AS TRIMCOLOR_CD FROM TBL_TRIMCOLOR2 WHERE TRIMCOLOR_NM = LEFT(MAIN_COLOR_VIEW_5,CHARINDEX('/',MAIN_COLOR_VIEW_5)-1) GROUP BY LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1)) AS MAIN_COLOR_VIEW_5,  ";
                cmd.CommandText += "            (SELECT  LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1) AS TRIMCOLOR_CD FROM TBL_TRIMCOLOR2 WHERE TRIMCOLOR_NM = LEFT(MAIN_COLOR_VIEW_6,CHARINDEX('/',MAIN_COLOR_VIEW_6)-1) GROUP BY LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1)) AS MAIN_COLOR_VIEW_6,  ";
                cmd.CommandText += "            (SELECT  LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1) AS TRIMCOLOR_CD FROM TBL_TRIMCOLOR2 WHERE TRIMCOLOR_NM = LEFT(MAIN_COLOR_VIEW_7,CHARINDEX('/',MAIN_COLOR_VIEW_7)-1) GROUP BY LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1)) AS MAIN_COLOR_VIEW_7,  ";
                cmd.CommandText += "            LEFT(MAIN_COLOR_VIEW_1, CHARINDEX('/', MAIN_COLOR_VIEW_1) - 1) AS MAIN_COLOR_VIEW_NM1, ";
                cmd.CommandText += "			LEFT(MAIN_COLOR_VIEW_2, CHARINDEX('/', MAIN_COLOR_VIEW_2) - 1) AS MAIN_COLOR_VIEW_NM2, ";
                cmd.CommandText += "			LEFT(MAIN_COLOR_VIEW_3, CHARINDEX('/', MAIN_COLOR_VIEW_3) - 1) AS MAIN_COLOR_VIEW_NM3, ";
                cmd.CommandText += "			LEFT(MAIN_COLOR_VIEW_4, CHARINDEX('/', MAIN_COLOR_VIEW_4) - 1) AS MAIN_COLOR_VIEW_NM4, ";
                cmd.CommandText += "			LEFT(MAIN_COLOR_VIEW_5, CHARINDEX('/', MAIN_COLOR_VIEW_5) - 1) AS MAIN_COLOR_VIEW_NM5, ";
                cmd.CommandText += "			LEFT(MAIN_COLOR_VIEW_6, CHARINDEX('/', MAIN_COLOR_VIEW_6) - 1) AS MAIN_COLOR_VIEW_NM6, ";
                cmd.CommandText += "			LEFT(MAIN_COLOR_VIEW_7, CHARINDEX('/', MAIN_COLOR_VIEW_7) - 1) AS MAIN_COLOR_VIEW_NM7  ";
                cmd.CommandText += "    FROM    TBL_RECOMMEND_TRIM ";
                cmd.CommandText += "    WHERE   ANSWER_1 = CASE WHEN CHARINDEX('주말', @usage) > 0 OR CHARINDEX('레저', @usage) > 0 OR CHARINDEX('레져', @usage) > 0 OR CHARINDEX('장거리', @usage) > 0 THEN '장거리' ";
                cmd.CommandText += "						ELSE '출퇴근' END ";
                cmd.CommandText += "    AND     ANSWER_2 = @importance ";
                cmd.CommandText += "    AND     ANSWER_3 = CASE WHEN CHARINDEX('여성', @genderAge) > 0 OR CHARINDEX('여자', @genderAge) > 0 OR CHARINDEX('여', @genderAge) > 0 OR CHARINDEX('female', @genderAge) > 0 OR CHARINDEX('woman', @genderAge) > 0 OR CHARINDEX('women', @genderAge) > 0 OR CHARINDEX('girl', @genderAge) > 0 THEN '여성' ";
                cmd.CommandText += "                            WHEN CHARINDEX('남성', @genderAge) > 0 OR CHARINDEX('남자', @genderAge) > 0 OR CHARINDEX('남', @genderAge) > 0 OR CHARINDEX('male', @genderAge) > 0 OR CHARINDEX('man', @genderAge) > 0 OR CHARINDEX('men', @genderAge) > 0 OR CHARINDEX('boy', @genderAge) > 0 THEN '남성' ";
                cmd.CommandText += "                            ELSE '기타' END ";
                cmd.CommandText += "    UNION ALL ";
                cmd.CommandText += "    SELECT  RECOMMEND_TITLE, ANSWER_1, ANSWER_2, ANSWER_3, ";
                cmd.CommandText += "             LEFT(TRIM_DETAIL, CHARINDEX('[', TRIM_DETAIL) - 2) AS TRIM_DETAIL, ";
                cmd.CommandText += "               SUBSTRING(TRIM_DETAIL, CHARINDEX('[', TRIM_DETAIL)+1, (CHARINDEX('/', TRIM_DETAIL)) - (CHARINDEX('[', TRIM_DETAIL) + 1)) AS TRIM_DETAIL_PRICE ";
                cmd.CommandText += "              , OPTION_1, OPTION_1_IMG_URL, OPTION_2, OPTION_2_IMG_URL, OPTION_3, OPTION_3_IMG_URL, OPTION_4, OPTION_4_IMG_URL, OPTION_5, OPTION_5_IMG_URL, ";
                cmd.CommandText += "            OPTION_6, OPTION_6_IMG_URL,  ";
                cmd.CommandText += "            (SELECT  LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1) AS TRIMCOLOR_CD FROM TBL_TRIMCOLOR2 WHERE TRIMCOLOR_NM = LEFT(MAIN_COLOR_VIEW_1,CHARINDEX('/',MAIN_COLOR_VIEW_1)-1) GROUP BY LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1)) AS MAIN_COLOR_VIEW_1,  ";
                cmd.CommandText += "            (SELECT  LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1) AS TRIMCOLOR_CD FROM TBL_TRIMCOLOR2 WHERE TRIMCOLOR_NM = LEFT(MAIN_COLOR_VIEW_2,CHARINDEX('/',MAIN_COLOR_VIEW_2)-1) GROUP BY LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1)) AS MAIN_COLOR_VIEW_2,  ";
                cmd.CommandText += "            (SELECT  LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1) AS TRIMCOLOR_CD FROM TBL_TRIMCOLOR2 WHERE TRIMCOLOR_NM = LEFT(MAIN_COLOR_VIEW_3,CHARINDEX('/',MAIN_COLOR_VIEW_3)-1) GROUP BY LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1)) AS MAIN_COLOR_VIEW_3,  ";
                cmd.CommandText += "            (SELECT  LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1) AS TRIMCOLOR_CD FROM TBL_TRIMCOLOR2 WHERE TRIMCOLOR_NM = LEFT(MAIN_COLOR_VIEW_4,CHARINDEX('/',MAIN_COLOR_VIEW_4)-1) GROUP BY LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1)) AS MAIN_COLOR_VIEW_4,  ";
                cmd.CommandText += "            (SELECT  LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1) AS TRIMCOLOR_CD FROM TBL_TRIMCOLOR2 WHERE TRIMCOLOR_NM = LEFT(MAIN_COLOR_VIEW_5,CHARINDEX('/',MAIN_COLOR_VIEW_5)-1) GROUP BY LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1)) AS MAIN_COLOR_VIEW_5,  ";
                cmd.CommandText += "            (SELECT  LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1) AS TRIMCOLOR_CD FROM TBL_TRIMCOLOR2 WHERE TRIMCOLOR_NM = LEFT(MAIN_COLOR_VIEW_6,CHARINDEX('/',MAIN_COLOR_VIEW_6)-1) GROUP BY LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1)) AS MAIN_COLOR_VIEW_6,  ";
                cmd.CommandText += "            (SELECT  LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1) AS TRIMCOLOR_CD FROM TBL_TRIMCOLOR2 WHERE TRIMCOLOR_NM = LEFT(MAIN_COLOR_VIEW_7,CHARINDEX('/',MAIN_COLOR_VIEW_7)-1) GROUP BY LEFT(TRIMCOLOR_CD, CHARINDEX(':',TRIMCOLOR_CD)-1)) AS MAIN_COLOR_VIEW_7,  ";
                cmd.CommandText += "            LEFT(MAIN_COLOR_VIEW_1, CHARINDEX('/', MAIN_COLOR_VIEW_1) - 1) AS MAIN_COLOR_VIEW_NM1, ";
                cmd.CommandText += "			LEFT(MAIN_COLOR_VIEW_2, CHARINDEX('/', MAIN_COLOR_VIEW_2) - 1) AS MAIN_COLOR_VIEW_NM2, ";
                cmd.CommandText += "			LEFT(MAIN_COLOR_VIEW_3, CHARINDEX('/', MAIN_COLOR_VIEW_3) - 1) AS MAIN_COLOR_VIEW_NM3, ";
                cmd.CommandText += "			LEFT(MAIN_COLOR_VIEW_4, CHARINDEX('/', MAIN_COLOR_VIEW_4) - 1) AS MAIN_COLOR_VIEW_NM4, ";
                cmd.CommandText += "			LEFT(MAIN_COLOR_VIEW_5, CHARINDEX('/', MAIN_COLOR_VIEW_5) - 1) AS MAIN_COLOR_VIEW_NM5, ";
                cmd.CommandText += "			LEFT(MAIN_COLOR_VIEW_6, CHARINDEX('/', MAIN_COLOR_VIEW_6) - 1) AS MAIN_COLOR_VIEW_NM6, ";
                cmd.CommandText += "			LEFT(MAIN_COLOR_VIEW_7, CHARINDEX('/', MAIN_COLOR_VIEW_7) - 1) AS MAIN_COLOR_VIEW_NM7  ";
                cmd.CommandText += "    FROM    TBL_RECOMMEND_TRIM ";
                cmd.CommandText += "    WHERE   ANSWER_1 = '기타' ";
                cmd.CommandText += ") A ";

                cmd.Parameters.AddWithValue("@usage", usage);
                cmd.Parameters.AddWithValue("@importance", importance);
                cmd.Parameters.AddWithValue("@genderAge", genderAge);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (rdr.Read())
                {
                    RecommendList dlg = new RecommendList();
                    dlg.RECOMMEND_TITLE = rdr["RECOMMEND_TITLE"] as string;
                    dlg.TRIM_DETAIL = rdr["TRIM_DETAIL"] as string;
                    dlg.TRIM_DETAIL_PRICE = rdr["TRIM_DETAIL_PRICE"] as string;
                    dlg.ANSWER_1 = rdr["ANSWER_1"] as string;
                    dlg.ANSWER_2 = rdr["ANSWER_2"] as string;
                    dlg.ANSWER_3 = rdr["ANSWER_3"] as string;
                    dlg.OPTION_1 = rdr["OPTION_1"] as string;
                    dlg.OPTION_1_IMG_URL = rdr["OPTION_1_IMG_URL"] as string;
                    dlg.OPTION_2 = rdr["OPTION_2"] as string;
                    dlg.OPTION_2_IMG_URL = rdr["OPTION_2_IMG_URL"] as string;
                    dlg.OPTION_3 = rdr["OPTION_3"] as string;
                    dlg.OPTION_3_IMG_URL = rdr["OPTION_3_IMG_URL"] as string;
                    dlg.OPTION_4 = rdr["OPTION_4"] as string;
                    dlg.OPTION_4_IMG_URL = rdr["OPTION_4_IMG_URL"] as string;
                    dlg.OPTION_5 = rdr["OPTION_5"] as string;
                    dlg.OPTION_5_IMG_URL = rdr["OPTION_5_IMG_URL"] as string;
                    //dlg.OPTION_6 = rdr["OPTION_6"] as string;
                    //dlg.OPTION_6_IMG_URL = rdr["OPTION_6_IMG_URL"] as string;
                    dlg.MAIN_COLOR_VIEW_1 = rdr["MAIN_COLOR_VIEW_1"] as string;
                    dlg.MAIN_COLOR_VIEW_2 = rdr["MAIN_COLOR_VIEW_2"] as string;
                    dlg.MAIN_COLOR_VIEW_3 = rdr["MAIN_COLOR_VIEW_3"] as string;
                    dlg.MAIN_COLOR_VIEW_4 = rdr["MAIN_COLOR_VIEW_4"] as string;
                    dlg.MAIN_COLOR_VIEW_5 = rdr["MAIN_COLOR_VIEW_5"] as string;
                    dlg.MAIN_COLOR_VIEW_6 = rdr["MAIN_COLOR_VIEW_6"] as string;
                    dlg.MAIN_COLOR_VIEW_7 = rdr["MAIN_COLOR_VIEW_7"] as string;
                    dlg.MAIN_COLOR_VIEW_NM1 = rdr["MAIN_COLOR_VIEW_NM1"] as string;
                    dlg.MAIN_COLOR_VIEW_NM2 = rdr["MAIN_COLOR_VIEW_NM2"] as string;
                    dlg.MAIN_COLOR_VIEW_NM3 = rdr["MAIN_COLOR_VIEW_NM3"] as string;
                    dlg.MAIN_COLOR_VIEW_NM4 = rdr["MAIN_COLOR_VIEW_NM4"] as string;
                    dlg.MAIN_COLOR_VIEW_NM5 = rdr["MAIN_COLOR_VIEW_NM5"] as string;
                    dlg.MAIN_COLOR_VIEW_NM6 = rdr["MAIN_COLOR_VIEW_NM6"] as string;
                    dlg.MAIN_COLOR_VIEW_NM7 = rdr["MAIN_COLOR_VIEW_NM7"] as string;

                    recommendList.Add(dlg);
                }

            }
            return recommendList;
        }



        public int SelectedRecommendCheck(int type, string kr_query, string eng_query)
        {
            SqlDataReader rdr = null;
            int result = 0;
            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText = "sp_selectrecommend";

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@kr_query", kr_query);
                cmd.Parameters.AddWithValue("@eng_query", eng_query);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                try
                {
                    while (rdr.Read())
                    {
                        result = Convert.ToInt32(rdr["CNT"]);
                    }
                } catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            return result;
        }


        public int SelectedRecommendMentCheck(string kr_query)
        {
            SqlDataReader rdr = null;
            int result = 0;
            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText = " select count(*) AS CNT from tbl_recommend where type!=1 and  kr_recommend = @kr_query ";

                cmd.Parameters.AddWithValue("@kr_query", kr_query);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                try
                {
                    while (rdr.Read())
                    {
                        result = Convert.ToInt32(rdr["CNT"]);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            return result;
        }


        public int SelectUserQueryErrorMessageCheck(string userID, int appID)
        {
            SqlDataReader rdr = null;
            int result = 0;
            //userID = arg.Replace("'", "''");
            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += " SELECT TOP 1 A.CHATBOT_COMMENT_CODE ";
                cmd.CommandText += " FROM ( ";
                cmd.CommandText += " 	SELECT  ";
                cmd.CommandText += " 		SID, ";
                cmd.CommandText += " 		CASE  CHATBOT_COMMENT_CODE  ";
                cmd.CommandText += " 			WHEN 'SEARCH' THEN '1' ";
                cmd.CommandText += " 			WHEN 'ERROR' THEN '1' ";
                cmd.CommandText += " 			ELSE '0' ";
                cmd.CommandText += " 		END CHATBOT_COMMENT_CODE ";
                cmd.CommandText += " 	FROM TBL_HISTORY_QUERY WHERE USER_NUMBER = '"+ userID + "' AND APP_ID = "+appID ;
                cmd.CommandText += " ) A ";
                cmd.CommandText += " ORDER BY A.SID DESC ";

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    result = Convert.ToInt32(rdr["CHATBOT_COMMENT_CODE"]);
                }
            }
            return result;
        }


        //추가 차종 선택

        public List<ChatBotAppList> SelectChatBotList(string appNM)
        {
            SqlDataReader rdr = null;
            List<ChatBotAppList> appList = new List<ChatBotAppList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                //cmd.CommandText = "SELECT DLG_ID, DLG_NM, DLG_MENT, DLG_LANG FROM TBL_DLG WHERE DLG_ID = @dlgID AND USE_YN = 'Y' AND DLG_ID > 999";
                cmd.CommandText = "SELECT APP_ID, LUIS_APP_ID, LUIS_SUBSCRIPTION_KEY FROM TBL_CHATBOT_APP WHERE APP_NM = @appNM ";

                cmd.Parameters.AddWithValue("@appNM", appNM);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {

                    int appID = Convert.ToInt32(rdr["APP_ID"]);
                    string luisID = rdr["LUIS_APP_ID"] as string;
                    string LuisSubKey = rdr["LUIS_SUBSCRIPTION_KEY"] as string;

                    ChatBotAppList app = new ChatBotAppList();
                    app.appId = appID;
                    app.luisAppId = luisID;
                    app.luisSubKey = LuisSubKey;


                    appList.Add(app);
                }
            }
            return appList;
        }


        public List<ChatBotAppList> SelectChatBotLuisList(int appId)
        {
            SqlDataReader rdr = null;
            List<ChatBotAppList> appList = new List<ChatBotAppList>();

            using (SqlConnection conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                //cmd.CommandText = "SELECT DLG_ID, DLG_NM, DLG_MENT, DLG_LANG FROM TBL_DLG WHERE DLG_ID = @dlgID AND USE_YN = 'Y' AND DLG_ID > 999";
                cmd.CommandText = "SELECT APP_ID, LUIS_APP_ID, LUIS_SUBSCRIPTION_KEY FROM TBL_CHATBOT_APP WHERE APP_ID = @appId ";

                cmd.Parameters.AddWithValue("@appId", appId);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    
                    int appID = Convert.ToInt32(rdr["APP_ID"]);
                    string luisID = rdr["LUIS_APP_ID"] as string;
                    string LuisSubKey = rdr["LUIS_SUBSCRIPTION_KEY"] as string;

                    ChatBotAppList app = new ChatBotAppList();
                    app.appId = appID;
                    app.luisAppId = luisID;
                    app.luisSubKey = LuisSubKey;


                    appList.Add(app);
                }
            }
            return appList;
        }

    }
}