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

namespace Bot_Application1.DB
{

    public class DbConnect
    {
        string connStr = "Data Source=faxtimedb.database.windows.net;Initial Catalog=taihoML;User ID=faxtime;Password=test2016!;";
        //string connStr = "Data Source=10.6.222.21,1433;Initial Catalog=KONA2_DB;User ID=konadb;Password=Didwoehd20-9;";
        StringBuilder sb = new StringBuilder();

        public void ConnectDb()
        {
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connStr);
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

        public List<LuisList> SelectLuis(string intent, String entities)
        {
            SqlDataReader rdr = null;
            List<LuisList> luis = new List<LuisList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText = "sp_selectdialogid_new";
                //cmd.CommandText = "sp_selectdialogid_new2";

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@intent", intent);
                cmd.Parameters.AddWithValue("@entities", entities);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    int dlgId = Convert.ToInt32(rdr["DLG_ID"]);

                    LuisList luis1 = new LuisList();
                    luis1.dlgId = dlgId;
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

            using (SqlConnection conn = new SqlConnection(connStr))
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

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT DLG_ID, DLG_NM, DLG_MENT, DLG_LANG FROM TBL_DLG WHERE DLG_ID = @dlgID AND USE_YN = 'Y' AND DLG_ID > 999";

                cmd.Parameters.AddWithValue("@dlgID", dlgID);

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

        public List<CardList> SelectDialogCard(int dlgID)
        {
            SqlDataReader rdr = null;
            List<CardList> dialogCard = new List<CardList>();

            using (SqlConnection conn = new SqlConnection(connStr))
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

        public List<ButtonList> SelectBtn(int dlgID, int cardID)
        {
            SqlDataReader rdr = null;
            List<ButtonList> button = new List<ButtonList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT DLG_ID, CARD_ID, BTN_ID, BTN_TYPE, BTN_TITLE, BTN_CONTEXT FROM TBL_DLG_BTN WHERE DLG_ID = @dlgID AND CARD_ID = @cardID AND USE_YN = 'Y' AND DLG_ID > 999";


                cmd.Parameters.AddWithValue("@dlgID", dlgID);
                cmd.Parameters.AddWithValue("@cardID", cardID);

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

            using (SqlConnection conn = new SqlConnection(connStr))
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

            using (SqlConnection conn = new SqlConnection(connStr))
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

            using (SqlConnection conn = new SqlConnection(connStr))
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

            using (SqlConnection conn = new SqlConnection(connStr))
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



        public int insertHistory(string userNumber, string customerCommentKR, string customerCommentEN, string chatbotCommentCode, string channel, int responseTime)
        {
            //SqlDataReader rdr = null;
            int result;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += " INSERT INTO TBL_HISTORY_QUERY ";
                cmd.CommandText += " (USER_NUMBER, CUSTOMER_COMMENT_KR, CUSTOMER_COMMENT_EN, CHATBOT_COMMENT_CODE, CHANNEL, RESPONSE_TIME, REG_DATE, ACTIVE_FLAG) ";
                cmd.CommandText += " VALUES ";
                cmd.CommandText += " (@userNumber, @customerCommentKR, @customerCommentEN, @chatbotCommentCode, @channel, @responseTime, CONVERT(VARCHAR, DATEADD(Hour, 9, GETDATE()), 101) + ' ' + CONVERT(VARCHAR, DATEADD(Hour, 9, GETDATE()), 24), 0) ";

                cmd.Parameters.AddWithValue("@userNumber", userNumber);
                cmd.Parameters.AddWithValue("@customerCommentKR", customerCommentKR);
                cmd.Parameters.AddWithValue("@customerCommentEN", customerCommentEN);
                cmd.Parameters.AddWithValue("@chatbotCommentCode", chatbotCommentCode);
                cmd.Parameters.AddWithValue("@channel", channel);
                cmd.Parameters.AddWithValue("@responseTime", responseTime);

                result = cmd.ExecuteNonQuery();
                Debug.WriteLine("result : " + result);
            }
            return result;
        }

        public List<TestDriverCenterList> SelectTestDriverDialog(string entitiesStr)
        {
            SqlDataReader rdr = null;
            List<TestDriverCenterList> dialog = new List<TestDriverCenterList>();
            String engTransferKor = "";

            engTransferKor = EngTransferKor.GetEngTransferKor(entitiesStr);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText = "  SELECT A.CTR_NM, A.CTR_ADDR, A.CTR_PHONE, A.CAR_DTL_INFO, A.MAP_X_TN, A.MAP_Y_TN, XRCL_CTY_NM, A.TRIMCOLOR_CD  " +
                                  "  FROM ( " +
                                  "  SELECT B.CTR_NM, B.CTR_ADDR, RTRIM(B.TH1_TN) + '-' + RTRIM(B.TH2_TN) + '-' + RTRIM(B.TH3_TN) AS CTR_PHONE, A.CAR_DTL_INFO, MAP_X_TN, MAP_Y_TN, XRCL_CTY_NM, C.TRIMCOLOR_CD  " +
                                  "  FROM dbo.TBL_CAR_MGMT A, TBL_CTR_MGMT B, (SELECT TRIMCOLOR_CD, TRIMCOLOR_NM FROM TBL_TRIMCOLOR2 GROUP BY  TRIMCOLOR_CD, TRIMCOLOR_NM HAVING RIGHT(TRIMCOLOR_NM,1) <> ')') C " +
                                  "  WHERE A.RBDA1_CD = B.CTR_CD " +
                                  "  AND A.SCN_CD = '2' " +
                                  "  AND B.CTR_ADDR LIKE '" + engTransferKor + "%' " +
                                  "  AND A.CARN LIKE '코나%' AND A.XRCL_CTY_NM = C.TRIMCOLOR_NM " +
                                  "  GROUP BY B.CTR_NM, B.CTR_ADDR, RTRIM(B.TH1_TN) + '-' + RTRIM(B.TH2_TN) + '-' + RTRIM(B.TH3_TN), A.CAR_DTL_INFO, MAP_X_TN, MAP_Y_TN, XRCL_CTY_NM, C.TRIMCOLOR_CD  " +
                                  "  UNION ALL " +
                                  "  SELECT B.CTR_NM, B.CTR_ADDR, RTRIM(B.TH1_TN) + '-' + RTRIM(B.TH2_TN) + '-' + RTRIM(B.TH3_TN) AS CTR_PHONE, A.CAR_DTL_INFO, MAP_X_TN, MAP_Y_TN, XRCL_CTY_NM, C.TRIMCOLOR_CD " +
                                  "  FROM dbo.TBL_CAR_MGMT A, TBL_CTR_MGMT B, (SELECT TRIMCOLOR_CD, TRIMCOLOR_NM FROM TBL_TRIMCOLOR2 GROUP BY  TRIMCOLOR_CD, TRIMCOLOR_NM HAVING RIGHT(TRIMCOLOR_NM,1) <> ')') C " +
                                  "  WHERE A.RBDA1_CD = B.CTR_CD " +
                                  "  AND A.SCN_CD = '2' " +
                                  "  AND CTR_NM = '" + engTransferKor + "' " +
                                  "  AND A.CARN LIKE '코나%' AND A.XRCL_CTY_NM = C.TRIMCOLOR_NM " +
                                  "  GROUP BY B.CTR_NM, B.CTR_ADDR, RTRIM(B.TH1_TN) + '-' + RTRIM(B.TH2_TN) + '-' + RTRIM(B.TH3_TN), A.CAR_DTL_INFO, MAP_X_TN, MAP_Y_TN, XRCL_CTY_NM, C.TRIMCOLOR_CD  ) A " +
                                  " GROUP BY A.CTR_NM, A.CTR_ADDR, A.CTR_PHONE, A.CAR_DTL_INFO, A.MAP_X_TN, A.MAP_Y_TN, A.XRCL_CTY_NM, A.TRIMCOLOR_CD  ";

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    //int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string dlgCtr_NM = rdr["CTR_NM"] as string;
                    string dlgCrt_Addr = rdr["CTR_ADDR"] as string;
                    string dlgCtr_Phone = rdr["CTR_PHONE"] as string;
                    string dlgCtr_Dtl_Info = rdr["CAR_DTL_INFO"] as string;
                    string dlgMap_X_Tn = rdr["MAP_X_TN"] as string;
                    string dlgMap_Y_Tn = rdr["MAP_Y_TN"] as string;
                    string dlgXrcl_Cty_NM = rdr["XRCL_CTY_NM"] as string;
                    string dlgTrim_Color_CD = rdr["TRIMCOLOR_CD"] as string;

                    TestDriverCenterList dlg = new TestDriverCenterList();
                    //dlg.dlgId = dlgId;
                    dlg.dlgCtrNM = dlgCtr_NM;
                    dlg.dlgCtrAddr = dlgCrt_Addr;
                    dlg.dlgCtrPhone = dlgCtr_Phone;
                    dlg.dlgCtrDtlInfo = dlgCtr_Dtl_Info;
                    dlg.dlgMapXTn = dlgMap_X_Tn;
                    dlg.dlgMapYTn = dlgMap_Y_Tn;
                    dlg.dlgXrclCtyNM = dlgXrcl_Cty_NM;
                    dlg.dlgdlgTrimColorCD = dlgTrim_Color_CD;


                    dialog.Add(dlg);
                }
            }
            return dialog;
        }

        public List<TestDriverColorList> SelectTestDriverColorDialog()
        {
            SqlDataReader rdr = null;
            List<TestDriverColorList> dialog = new List<TestDriverColorList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText = "SELECT B.BR_NM, B.BR_ADDR, B.BR_CCPC, REPLACE(A.XRCL_CTY_NM, ' ',''), B.BR_XCOO, B.BR_YCOO " +
                                  " FROM dbo.TBL_CAR_MGMT A, TBL_BR_MGMT B " +
                                  "  WHERE A.DSP_BR_CD = B.BR_CD " +
                                  "  AND A.SCN_CD = '1' " +
                                  "  AND A.CARN LIKE '코나%' " +
                                  //"  AND B.BR_ADDR LIKE '서울%' " +
                                  //"  AND REPLACE(A.XRCL_CTY_NM, ' ','') = REPLACE('애쉬 블루', ' ', '') " +
                                  "  GROUP BY B.BR_NM, B.BR_ADDR, B.BR_CCPC, REPLACE(A.XRCL_CTY_NM, ' ', ''), B.BR_XCOO, B.BR_YCOO ";

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {

                    string dlgBr_NM = rdr["BR_NM"] as string;
                    string dlgBr_Addr = rdr["BR_ADDR"] as string;
                    string dlgBr_Ccpc = rdr["BR_CCPC"] as string;
                    string dlgBr_Xcoo = rdr["BR_XCOO"] as string;
                    string dlgBr_Ycoo = rdr["BR_YCOO"] as string;

                    TestDriverColorList dlg = new TestDriverColorList();

                    dlg.dlgBrNM = dlgBr_NM;
                    dlg.dlgBrAddr = dlgBr_Addr;
                    dlg.dlgBrCcpc = dlgBr_Ccpc;
                    dlg.dlgBrXcoo = dlgBr_Xcoo;
                    dlg.dlgBrYcoo = dlgBr_Ycoo;


                    dialog.Add(dlg);
                }
            }
            return dialog;
        }

        public List<CarColorList> SelectCarColorListDialog()
        {
            SqlDataReader rdr = null;
            List<CarColorList> dialog = new List<CarColorList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                //cmd.CommandText = "SELECT DLG_ID, DLG_NM, DLG_MENT, DLG_LANG FROM TBL_DLG WHERE DLG_ID = '" + dlgID  + "' AND USE_YN = 'Y'";
                /*
                cmd.CommandText = " SELECT REPLACE(A.XRCL_CTY_NM, ' ','') AS XRCL_CTY_NM, COUNT(*) AS CNT " +
                                  " FROM dbo.TBL_CAR_MGMT A, TBL_BR_MGMT B  " +
                                  " WHERE A.DSP_BR_CD = B.CTR_CD " +
                                  " AND A.SCN_CD = '1' " +
                                  " AND A.CARN LIKE '투싼%'  " +
                                  " GROUP BY REPLACE(A.XRCL_CTY_NM, ' ', '') ";
                                  */

                cmd.CommandText = " SELECT REPLACE(A.XRCL_CTY_NM, ' ','') AS XRCL_CTY_NM, TRIMCOLOR_CD, COUNT(*) AS CNT " +
                                 " FROM dbo.TBL_CAR_MGMT A, TBL_BR_MGMT B, (SELECT TRIMCOLOR_CD, TRIMCOLOR_NM FROM TBL_TRIMCOLOR2 WHERE RIGHT(TRIMCOLOR_NM,1) <> ')' GROUP BY TRIMCOLOR_CD, TRIMCOLOR_NM ) C " +
                                 " WHERE A.DSP_BR_CD = B.BR_CD " +
                                 " AND A.XRCL_CTY_NM = C.TRIMCOLOR_NM " +
                                 " AND A.SCN_CD = '1' " +
                                 " AND A.CARN LIKE '코나%'  " +
                                 " GROUP BY TRIMCOLOR_CD, REPLACE(A.XRCL_CTY_NM, ' ', '') ";

                //cmd.Parameters.AddWithValue("@dlgID", dlgID);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    //int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string dlgXrcl_Cty_NM = rdr["XRCL_CTY_NM"] as string;
                    string dlgTrimColor_CD = rdr["TRIMCOLOR_CD"] as string;
                    int dlgCnt = Convert.ToInt32(rdr["CNT"]);

                    CarColorList dlg = new CarColorList();
                    dlg.dlgXrclCtyNM = dlgXrcl_Cty_NM;
                    dlg.dlgTrimColorCD = dlgTrimColor_CD;
                    dlg.dlCnt = dlgCnt;

                    dialog.Add(dlg);
                }
            }
            return dialog;
        }

        public List<CarColorAreaList> SelectCarColorAreaListDialog(string str)
        {
            SqlDataReader rdr = null;
            List<CarColorAreaList> dialog = new List<CarColorAreaList>();

            //str = EngTransferKor.GetEngTransferKor(str);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                //cmd.CommandText = "SELECT DLG_ID, DLG_NM, DLG_MENT, DLG_LANG FROM TBL_DLG WHERE DLG_ID = '" + dlgID  + "' AND USE_YN = 'Y'";
                cmd.CommandText = " SELECT A.BR_DTL_ADDR1 AS ADDR " +
                                    " FROM " +
                                    " (SELECT BR_DTL_ADDR1, COUNT(*) AS CNT " +
                                    " FROM dbo.TBL_CAR_MGMT A, TBL_BR_MGMT B " +
                                    " WHERE A.DSP_BR_CD = B.BR_CD " +
                                    " AND A.SCN_CD = '1' " +
                                    " AND A.CARN LIKE '코나%'  " +
                                    " AND A.XRCL_CTY_NM = '" + str + "' " +
                                    " GROUP BY BR_DTL_ADDR1 ) A " +
                                    " ORDER BY A.CNT DESC  ";

                //cmd.Parameters.AddWithValue("@dlgID", dlgID);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    //int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string dlgAddr = rdr["ADDR"] as string;

                    CarColorAreaList dlg = new CarColorAreaList();
                    dlg.dlgAddr = dlgAddr;

                    dialog.Add(dlg);
                }
            }
            return dialog;
        }
        //대리점 리스트
        public List<CarBranchList> SelectCarBranchListDialog(string area, string color)
        {
            SqlDataReader rdr = null;
            List<CarBranchList> dialog = new List<CarBranchList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                //cmd.CommandText = "SELECT DLG_ID, DLG_NM, DLG_MENT, DLG_LANG FROM TBL_DLG WHERE DLG_ID = '" + dlgID  + "' AND USE_YN = 'Y'";
                cmd.CommandText = " SELECT B.BR_NM, BR_ADDR, B.BR_CCPC " +
                                  " FROM dbo.TBL_CAR_MGMT A, TBL_BR_MGMT B " +
                                  " WHERE A.DSP_BR_CD = B.BR_CD " +
                                  " AND A.XRCL_CTY_NM = '" + color + "' " +
                                  //" AND A.XRCL_CTY_NM = '" & color & "' " +                                   
                                  " AND A.SCN_CD = '1' " +
                                  " AND A.CARN LIKE '코나%' " +
                                  " AND B.BR_DTL_ADDR1 = '" + area + "' " +
                                  " GROUP BY B.BR_NM, B.BR_CCPC, BR_ADDR ";

                //cmd.Parameters.AddWithValue("@dlgID", dlgID);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    //int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string dlgBr_NM = rdr["BR_NM"] as string;
                    string dlgBr_Addr = rdr["BR_ADDR"] as string;
                    string dlgBr_Ccpc = rdr["BR_CCPC"] as string;

                    CarBranchList dlg = new CarBranchList();
                    dlg.dlgBrNM = dlgBr_NM;
                    dlg.dlgBrAddr = dlgBr_Addr;
                    dlg.dlgDspBrTN = dlgBr_Ccpc;

                    dialog.Add(dlg);
                }
            }
            return dialog;
        }

        //대리점 정보
        public List<CarBranchInfo> SelectCarBranchDialog(string msg)
        {
            SqlDataReader rdr = null;
            List<CarBranchInfo> dialog = new List<CarBranchInfo>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                //cmd.CommandText = "SELECT DLG_ID, DLG_NM, DLG_MENT, DLG_LANG FROM TBL_DLG WHERE DLG_ID = '" + dlgID  + "' AND USE_YN = 'Y'";
                /*
                if(gubun.Equals("branch"))
                {
                    cmd.CommandText = " SELECT B.BR_NM, BR_ADDR, B.BR_CCPC, B.BR_XCOO, B.BR_YCOO  " +
                                  " FROM dbo.TBL_CAR_MGMT A, TBL_BR_MGMT B " +
                                  //" WHERE A.DSP_BR_CD = B.CTR_CD " +
                                  " WHERE A.SCN_CD = '1' " +
                                  " AND A.CARN LIKE '코나%' " +
                                  " AND B.BR_NM = '" + area + "' " +
                                  " GROUP BY B.BR_NM, B.BR_CCPC, BR_ADDR, B.BR_XCOO, B.BR_YCOO  ";
                } else
                {
                    cmd.CommandText = " SELECT B.CTR_NM AS BR_NM, B.CTR_ADDR AS BR_ADDR, RTRIM(B.TH1_TN) + '-' + RTRIM(B.TH2_TN) + '-' + RTRIM(B.TH3_TN) AS BR_CCPC, MAP_X_TN AS BR_XCOO, MAP_Y_TN AS BR_YCOO" +
                                  " FROM dbo.TBL_CAR_MGMT A, TBL_CTR_MGMT B " +
                                  " WHERE A.RBDA1_CD = B.CTR_CD " +
                                  " AND A.SCN_CD = '2' " +
                                  " AND A.CARN LIKE '코나%' " +
                                  " AND B.CTR_NM LIKE '" + area + "%' " +
                                  " GROUP BY B.CTR_NM, B.CTR_ADDR, RTRIM(B.TH1_TN) + '-' + RTRIM(B.TH2_TN) + '-' + RTRIM(B.TH3_TN),MAP_X_TN, MAP_Y_TN ";
                }
                *
                */
                cmd.CommandText = " SELECT B.BR_NM, BR_ADDR, B.BR_CCPC, B.BR_XCOO, B.BR_YCOO  " +
                                  " FROM dbo.TBL_CAR_MGMT A, TBL_BR_MGMT B " +
                                  " WHERE A.DSP_BR_CD = B.BR_CD " +
                                  " AND A.SCN_CD = '1' " +
                                  " AND A.CARN LIKE '코나%' " +
                                  " AND B.BR_NM = ( " +
                                  "     SELECT TOP 1 TBL.BRANCH_NM " +
                                  "     FROM(" +
                                  "         SELECT A.BRANCH_NM, LEN(A.BRANCH_ENG) AS ENG_LEN, " +
                                  "             CASE WHEN CHARINDEX(A.BRANCH_ENG, '" + msg.ToLower() + "') > 0 THEN 1 ELSE 0 END AS CNT " +
                                  "         FROM TBL_BRANCH A " +
                                  "         ) TBL " +
                                  "     WHERE   TBL.CNT > 0  ORDER BY TBL.ENG_LEN DESC " +
                                  //"     AND TBL.LENGTH1 > 2" +
                                  ") " +
                                  " GROUP BY B.BR_NM, B.BR_CCPC, BR_ADDR, B.BR_XCOO, B.BR_YCOO  ";


                //cmd.Parameters.AddWithValue("@dlgID", dlgID);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    //int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string dlgBr_NM = rdr["BR_NM"] as string;
                    string dlgBr_Addr = rdr["BR_ADDR"] as string;
                    string dlgBr_Ccpc = rdr["BR_CCPC"] as string;
                    string dlgBr_Xcoo = rdr["BR_XCOO"] as string;
                    string dlgBr_Ycoo = rdr["BR_YCOO"] as string;

                    CarBranchInfo dlg = new CarBranchInfo();
                    dlg.dlgBrNM = dlgBr_NM;
                    dlg.dlgBrAddr = dlgBr_Addr;
                    dlg.dlgDspBrTN = dlgBr_Ccpc;
                    dlg.dlgBrXcoo = dlgBr_Xcoo;
                    dlg.dlgBrYcoo = dlgBr_Ycoo;

                    dialog.Add(dlg);
                }
            }
            return dialog;
        }

        //지역별 시승센터 조회
        public List<AreaTestCenterList> SelectAreaTestCenterListDialog(string area)
        {
            SqlDataReader rdr = null;
            List<AreaTestCenterList> dialog = new List<AreaTestCenterList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                //cmd.CommandText = "SELECT DLG_ID, DLG_NM, DLG_MENT, DLG_LANG FROM TBL_DLG WHERE DLG_ID = '" + dlgID  + "' AND USE_YN = 'Y'";
                cmd.CommandText = " SELECT B.CTR_NM, B.CTR_ADDR, RTRIM(B.TH1_TN) + '-' + RTRIM(B.TH2_TN) + '-' + RTRIM(B.TH3_TN) AS CTR_PHONE " +
                                  " FROM dbo.TBL_CAR_MGMT A, TBL_CTR_MGMT B " +
                                  " WHERE A.RBDA1_CD = B.CTR_CD " +
                                  " AND A.SCN_CD = '2' " +
                                  " AND A.CARN LIKE '코나%' " +
                                  " AND B.CTR_ADDR LIKE '" + area + "%' " +
                                  " GROUP BY B.CTR_NM, B.CTR_ADDR, RTRIM(B.TH1_TN) + '-' + RTRIM(B.TH2_TN) + '-' + RTRIM(B.TH3_TN) ";

                //cmd.Parameters.AddWithValue("@dlgID", dlgID);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    //int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string dlgCtr_NM = rdr["CTR_NM"] as string;
                    string dlgCrt_Addr = rdr["CTR_ADDR"] as string;
                    string dlgCtr_Phone = rdr["CTR_PHONE"] as string;

                    AreaTestCenterList dlg = new AreaTestCenterList();
                    dlg.dlgCtrNM = dlgCtr_NM;
                    dlg.dlgCtrAddr = dlgCrt_Addr;
                    dlg.dlgCtrPhone = dlgCtr_Phone;

                    dialog.Add(dlg);
                }
            }
            return dialog;
        }

        //지역별 시승센터 갯수 리스트 조회
        public List<AreaTestCenterCountList> SelectAreaTestCenterCountListDialog()
        {
            SqlDataReader rdr = null;
            List<AreaTestCenterCountList> dialog = new List<AreaTestCenterCountList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                //cmd.CommandText = "SELECT DLG_ID, DLG_NM, DLG_MENT, DLG_LANG FROM TBL_DLG WHERE DLG_ID = '" + dlgID  + "' AND USE_YN = 'Y'";
                cmd.CommandText = " SELECT A.AREANM, A.AREALIST, A.AREACNT " +
                                    " FROM " +
                                    " 	(	 " +
                                    " 	SELECT TBL_ADDR.ADDR AS AREANM, TBL_ADDR.ADDR1 AS AREALIST, (SELECT LEN(TBL_ADDR.ADDR1)-LEN(REPLACE(TBL_ADDR.ADDR1,',',''))) +1  AS AREACNT  " +
                                    " 	FROM  " +
                                    " 	   (  " +
                                    " 		SELECT TBL.ADDR, STUFF((  " +
                                    " 							 SELECT  ',' + B.CTR_NM  " +
                                    " 							 FROM    TBL_CAR_MGMT A, TBL_CTR_MGMT B  " +
                                    " 							 WHERE   A.RBDA1_CD = B.CTR_CD  " +
                                    " 							 AND     A.SCN_CD = '2'  " +
                                    " 							 AND     A.CARN LIKE '코나%'  " +
                                    " 							 AND     REPLACE(LEFT(B.CTR_ADDR, CHARINDEX(' ', B.CTR_ADDR)), '경기도', '경기') = REPLACE(LEFT(TBL.ADDR, CHARINDEX(' ', TBL.ADDR)), '경기도', '경기')  " +
                                    " 							 FOR XML PATH('')), 1, 1, '') AS ADDR1  " +
                                    " 		FROM  " +
                                    " 		   (  " +
                                    " 		   SELECT  REPLACE(LEFT(B.CTR_ADDR, CHARINDEX(' ', B.CTR_ADDR)), '경기도', '경기') AS ADDR, B.CTR_NM  " +
                                    " 		   FROM    TBL_CAR_MGMT A, TBL_CTR_MGMT B  " +
                                    " 		   WHERE   A.RBDA1_CD = B.CTR_CD  " +
                                    " 		   AND     A.SCN_CD = '2'  " +
                                    " 		   AND     A.CARN LIKE '코나%'  " +
                                    " 		   ) TBL  " +
                                    " 		) TBL_ADDR  " +
                                    " 	GROUP BY TBL_ADDR.ADDR, TBL_ADDR.ADDR1  " +
                                    " 	) A  " +
                                    " ORDER BY A.AREACNT DESC  ";

                //cmd.Parameters.AddWithValue("@dlgID", dlgID);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    //int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string dlgArea_NM = rdr["AREANM"] as string;
                    string dlgArea_List = rdr["AREALIST"] as string;
                    int dlgArea_Cnt = Convert.ToInt32(rdr["AREACNT"]);

                    string[] dlgArea_List_Result = System.Text.RegularExpressions.Regex.Split(dlgArea_List, ",");
                    string result = "";

                    int dlgArea_List_Result_cnt = 0;
                    foreach (string s in dlgArea_List_Result)
                    {
                        dlgArea_List_Result_cnt += 1;
                        result += s + ",";

                        if (dlgArea_List_Result_cnt == 2)
                        {
                            break;
                        }
                    }
                    result = result.Substring(0, result.Length - 1);

                    AreaTestCenterCountList dlg = new AreaTestCenterCountList();
                    dlg.dlgAreaNM = dlgArea_NM;
                    dlg.dlgAreaList = result;
                    dlg.dlgAreaCnt = dlgArea_Cnt;

                    dialog.Add(dlg);
                }
            }
            return dialog;
        }















        //견적 쿼리

        public List<CarModelList> SelectCarModelList()
        {
            SqlDataReader rdr = null;
            List<CarModelList> carModel = new List<CarModelList>();

            using (SqlConnection conn = new SqlConnection(connStr))
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

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += " SELECT CAR_TRIM_NM																												";
                cmd.CommandText += " 	 , SALE_CD                                                                                                                  ";
                cmd.CommandText += " 	 , FNUM_CD                                                                                                                  ";
                cmd.CommandText += " 	 , SALE_PRICE   ,FUEL, DRIVEWHEEL, TUIX, COLORPACKAGE, CARTRIM                                                              ";
                cmd.CommandText += "  FROM                                                                                                                          ";
                cmd.CommandText += " (                                                                                                                              ";
                cmd.CommandText += "      SELECT                                                                                                                    ";
                cmd.CommandText += " 	 (                                                                                                                          ";
                cmd.CommandText += " 		 SELECT MAX(CASE WHEN POS = 2 AND VAL1 != '터보' THEN VAL1 + ' ' ELSE '' END)  +                                        ";
                cmd.CommandText += " 			   MAX(CASE WHEN POS = 4 AND VAL1 != '터보' AND VAL1 != ('오토')  THEN VAL1 + ' ' ELSE '' END)+                     ";
                cmd.CommandText += " 			   MAX(CASE WHEN POS = 5  AND VAL1 != ('오토')  THEN VAL1 + ' ' ELSE '' END) +                                      ";
                cmd.CommandText += " 			   MAX(CASE WHEN POS = 6  AND VAL1 != ('오토')  THEN VAL1 + ' ' ELSE '' END)+                                       ";
                cmd.CommandText += " 			   MAX(CASE WHEN POS = 7  AND VAL1 != ('오토')  THEN VAL1 + ' ' ELSE '' END)+                                       ";
                cmd.CommandText += " 			   MAX(CASE WHEN POS = 8  AND VAL1 != ('오토')  THEN VAL1 + ' ' ELSE '' END)+                                       ";
                cmd.CommandText += " 			   MAX(CASE WHEN POS = 9  AND VAL1 != ('오토')  THEN VAL1 + ' ' ELSE '' END)+                                       ";
                cmd.CommandText += " 			   MAX(CASE WHEN POS = 10 AND VAL1 != ('오토')  THEN VAL1 + ' ' ELSE '' END)+                                       ";
                cmd.CommandText += " 			   MAX(CASE WHEN POS = 11 AND VAL1 != ('오토')  THEN VAL1 + ' ' ELSE '' END) CAR_TRIM                               ";
                cmd.CommandText += " 		 FROM FN_SPLIT(REPLACE(B.MODEL_NM ,' ','-'),'-') A                                                                      ";
                cmd.CommandText += " 	 ) CAR_TRIM_NM                                                                                                              ";
                cmd.CommandText += " 	 , SALE_CD                                                                                                                  ";
                cmd.CommandText += " 	 , FNUM_CD                                                                                                                  ";
                cmd.CommandText += " 	 , SALE_PRICE   ,FUEL, DRIVEWHEEL, TUIX, COLORPACKAGE, CARTRIM                                                              ";
                cmd.CommandText += " 	 FROM TBL_CARMODEL B                                                                                                        ";
                cmd.CommandText += " 	 WHERE CAR_CD_TYPE IN                                                                                                       ";
                cmd.CommandText += " 	 (                                                                                                                          ";
                cmd.CommandText += " 		SELECT CAR_CD_TYPE                                                                                                      ";
                cmd.CommandText += " 		  FROM TBL_CAR_MODEL_DEF                                                                                                ";
                cmd.CommandText += " 		 where SUBSTRING(CAR_NAME,CHARINDEX(' ',CAR_NAME)+1,LEN(CAR_NAME)) = @modelNm                                           ";
                cmd.CommandText += " 	 ) AND SALE_CD NOT LIKE ('OSJX%')                                                                                           ";
                cmd.CommandText += " ) A                                                                                                                            ";
                cmd.CommandText += "  GROUP BY CAR_TRIM_NM                                                                                                          ";
                cmd.CommandText += " 	 , SALE_CD                                                                                                                  ";
                cmd.CommandText += " 	 , FNUM_CD                                                                                                                  ";
                cmd.CommandText += " 	 , SALE_PRICE  ,FUEL, DRIVEWHEEL, TUIX, COLORPACKAGE, CARTRIM                                                               ";

                cmd.Parameters.AddWithValue("@modelNm", modelNm);

                Debug.WriteLine("query : " + cmd);


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

        public List<CarTrimList> SelectCarTrimList1(string modelNm)
        {
            SqlDataReader rdr = null;
            List<CarTrimList> carTrimList = new List<CarTrimList>();

            Debug.WriteLine("SelectCarTrimList1 : ");

            using (SqlConnection conn = new SqlConnection(connStr))
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

        public List<CarExColorList> SelectCarExColorAllList()
        {
            SqlDataReader rdr = null;
            List<CarExColorList> carExColorList = new List<CarExColorList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += " SELECT TRIMCOLOR_NM	";
                cmd.CommandText += "       ,TRIMCOLOR_CD	";
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


        public List<CarInColorList> SelectCarInColorAllList()
        {
            SqlDataReader rdr = null;
            List<CarInColorList> carInColorList = new List<CarInColorList>();

            using (SqlConnection conn = new SqlConnection(connStr))
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



        public List<CarExColorList> SelectCarExColorList(string modelNm)
        {
            SqlDataReader rdr = null;
            List<CarExColorList> carExColorList = new List<CarExColorList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += " SELECT TRIMCOLOR_NM											";
                cmd.CommandText += "      , TRIMCOLOR_CD                                            ";
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

            using (SqlConnection conn = new SqlConnection(connStr))
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


        public List<CarOptionList> SelectOptionList(string msg)
        {
            SqlDataReader rdr = null;
            List<CarOptionList> carOptionList = new List<CarOptionList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "  SELECT OPT_NM                                         ";
                cmd.CommandText += "      , OPT_PRICE ,OPT_CD       , PKG_DT                ";
                cmd.CommandText += "  FROM FN_PRICE_OPTION                                  ";
                cmd.CommandText += "  ('" + msg + "')                                        ";

                cmd.Parameters.AddWithValue("@msg", msg);


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

            using (SqlConnection conn = new SqlConnection(connStr))
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

            using (SqlConnection conn = new SqlConnection(connStr))
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

            using (SqlConnection conn = new SqlConnection(connStr))
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

            using (SqlConnection conn = new SqlConnection(connStr))
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



        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Query Analysis
        // Check if dialogue already exists, return Luis format JSON.
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public JObject SelectQueryAnalysis(string query)
        {
            String json = @"{
                'entities':'',
                'intents':[{'intent':''}],
                'test_driveWhere':'',
                'car_priceWhere':'',
                'car_option':''
            }";
            JObject returnJson = JObject.Parse(json);

            SqlDataReader rdr = null;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                //cmd.CommandText = "SELECT TOP 1 INTENT_ID, ENTITIES_IDS, ISNULL(CAR_COLOR,'') AS CAR_COLOR, ISNULL(CAR_AREA,'') AS CAR_AREA  ,ISNULL(CAR_PRICEWHERE,'') AS CAR_PRICEWHERE  ,ISNULL(CAR_OPTION,'') AS CAR_OPTION FROM TBL_QUERY_ANALYSIS_RESULT2 WHERE QUERY = @query AND RESULT = 'H'";
                cmd.CommandText = "SELECT TOP 1 INTENT_ID, ENTITIES_IDS, ISNULL(CAR_COLOR,'') AS CAR_COLOR, ISNULL(CAR_AREA,'') AS CAR_AREA  ,ISNULL(CAR_PRICEWHERE,'') AS CAR_PRICEWHERE  ,ISNULL(CAR_OPTION,'') AS CAR_OPTION FROM TBL_QUERY_ANALYSIS_RESULT WHERE QUERY = @query AND RESULT = 'H'";
                cmd.Parameters.AddWithValue("@query", query.Trim().ToLower());
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    returnJson["entities"] = rdr["ENTITIES_IDS"] as string;
                    returnJson["intents"][0]["intent"] = rdr["INTENT_ID"] as string;
                    returnJson["test_driveWhere"] = rdr["CAR_COLOR"] as string;
                    returnJson["car_area"] = rdr["CAR_AREA"] as string;
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
        public int insertUserQuery(string query, string intentID, string entitiesIDS, int luisID, char result, string car_area, string car_colorArea, string car_priceWhere, string car_option)
        {
            int dbResult = 0;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                /*
                cmd.CommandText += " INSERT INTO TBL_QUERY_ANALYSIS_RESULT ";
                cmd.CommandText += " (QUERY, INTENT_ID, ENTITIES_IDS, LUIS_ID, RESULT, CAR_COLOR, CAR_AREA) ";
                cmd.CommandText += " VALUES ";
                cmd.CommandText += " (@query, @intentID, @entitiesIDS, @luisID, @result, @car_color, @car_area) ";
                */
                //cmd.CommandText = "sp_insertusehistory2";
                cmd.CommandText = "sp_insertusehistory";

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@query", query.Trim().ToLower());
                cmd.Parameters.AddWithValue("@intentID", intentID.Trim());
                cmd.Parameters.AddWithValue("@entitiesIDS", entitiesIDS.Trim().ToLower());
                cmd.Parameters.AddWithValue("@luisID", luisID);
                cmd.Parameters.AddWithValue("@result", result);
                cmd.Parameters.AddWithValue("@car_color", car_colorArea);
                cmd.Parameters.AddWithValue("@car_area", car_area);
                cmd.Parameters.AddWithValue("@car_priceWhere", car_priceWhere);
                cmd.Parameters.AddWithValue("@car_option", car_option);


                dbResult = cmd.ExecuteNonQuery();
            }
            return dbResult;
        }

        public List<TestDriveLuisResult> SelectTestDriveLuisResult(String arg)
        {
            SqlDataReader rdr = null;

            List<TestDriveLuisResult> testDriveLuisResult = new List<TestDriveLuisResult>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "    SELECT INTENT, ENTITY, ENTITY_VALUE ";
                cmd.CommandText += "    FROM ";
                cmd.CommandText += "        FN_LUIS_RESULT_DRIVE ";
                cmd.CommandText += "        (N'" + arg.Replace("'", "''") + "') ";

                //cmd.Parameters.AddWithValue("@arg", arg);

                Debug.WriteLine("query : " + cmd.CommandText);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    //int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string dlgIntent = rdr["INTENT"] as string;
                    string dlgEntity = rdr["ENTITY"] as string;
                    string dlgEntityValue = rdr["ENTITY_VALUE"] as string;

                    TestDriveLuisResult dlg = new TestDriveLuisResult();
                    dlg.intent = dlgIntent;
                    dlg.entity = dlgEntity;
                    dlg.entity_value = dlgEntityValue;

                    testDriveLuisResult.Add(dlg);
                }
            }
            return testDriveLuisResult;
        }

        public List<TestDriveList> SelectTestDriveList(String arg)
        {
            SqlDataReader rdr = null;

            List<TestDriveList> testDriveList = new List<TestDriveList>();

            using (SqlConnection conn = new SqlConnection(connStr))
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
    }
}