﻿Imports System.IO.Ports
Imports STRGS = Microsoft.VisualBasic.Strings

Public Class plcbus
    Public port As New System.IO.Ports.SerialPort
    Private port_ouvert As Boolean = False
    Private ackreceived As Boolean = False
    Private port_name As String = ""
    'Private table_ack As New DataTable

    Dim com_to_hex As New Dictionary(Of String, String)
    Dim hex_to_com As New Dictionary(Of String, String)

    Private BufferIn(8192) As Byte
    Private firstbyte As Boolean = True
    Private bytecnt As Integer = 0
    Private recbuf(30) As Byte

    Public Sub New()
        com_to_hex.Add("ALL_UNITS_OFF", 0)
        com_to_hex.Add("ALL_LIGHTS_ON", 1)
        com_to_hex.Add("ON", 2) 'data1 must be 100, data2 must be 0
        com_to_hex.Add("OFF", 3) 'data1 must be 0, data2 must be 0
        com_to_hex.Add("DIM", 4) 'light will dim until fade-stop func=11 is received /  data1 = Fade rate
        com_to_hex.Add("BRIGHT", 5) 'light will bright until fade-stop func=11 is received /  data1 = Fade rate
        com_to_hex.Add("ALL_LIGHTS_OFF", 6)
        com_to_hex.Add("All_USER_LIGHTS_ON", 7)
        com_to_hex.Add("All_USER_UNITS_OFF", 8)
        com_to_hex.Add("All_USER_LIGHTS_OFF", 9)
        com_to_hex.Add("BLINK", 10) 'data1=interval
        com_to_hex.Add("FADE_STOP", 11)
        com_to_hex.Add("PRESET_DIM", 12) 'data1=dim level, data2=rate
        com_to_hex.Add("STATUS_ON", 13)
        com_to_hex.Add("STATUS_OFF", 14)
        com_to_hex.Add("STATUS_REQUEST", 15)
        com_to_hex.Add("ReceiverMasterAddressSetup", 16) 'data1=New user code, data2=new home+unitcode
        com_to_hex.Add("TransmitterMasterAddressSetup", 17) 'data1=New user code, data2=new home+unitcode
        com_to_hex.Add("SceneAddressSetup", 18)
        com_to_hex.Add("SceneAddressErase", 19)
        com_to_hex.Add("AllSceneAddressErase", 20)
        com_to_hex.Add("Reserved1", 21)
        com_to_hex.Add("Reserved2", 22)
        com_to_hex.Add("Reserved3", 23)
        com_to_hex.Add("GetSignalStrength", 24) 'data1=signal strength
        com_to_hex.Add("GetNoiseStrength", 25) 'data1=Noise strength
        com_to_hex.Add("ReportSignalStrength", 26)
        com_to_hex.Add("ReportNoiseStrength", 27)
        com_to_hex.Add("GetAllIdPulse", 28)
        com_to_hex.Add("GetOnlyOnIdPulse", 29)
        com_to_hex.Add("ReportAllIdPulse3Phase", 30)
        com_to_hex.Add("ReportOnlyOnIdPulse3Phase", 31)

        hex_to_com.Add(0, "ALL_UNITS_OFF")
        hex_to_com.Add(1, "ALL_LIGHTS_ON")
        hex_to_com.Add(2, "ON")
        hex_to_com.Add(3, "OFF")
        hex_to_com.Add(4, "DIM")
        hex_to_com.Add(5, "BRIGHT")
        hex_to_com.Add(6, "ALL_LIGHTS_OFF")
        hex_to_com.Add(7, "All_USER_LIGHTS_ON")
        hex_to_com.Add(8, "All_USER_UNITS_OFF")
        hex_to_com.Add(9, "All_USER_LIGHTS_OFF")
        hex_to_com.Add(10, "BLINK")
        hex_to_com.Add(11, "FADE_STOP")
        hex_to_com.Add(12, "PRESET_DIM")
        hex_to_com.Add(13, "STATUS_ON")
        hex_to_com.Add(14, "STATUS_OFF")
        hex_to_com.Add(15, "STATUS_REQUEST")
        hex_to_com.Add(16, "ReceiverMasterAddressSetup")
        hex_to_com.Add(17, "TransmitterMasterAddressSetup")
        hex_to_com.Add(18, "SceneAddressSetup")
        hex_to_com.Add(19, "SceneAddressErase")
        hex_to_com.Add(20, "AllSceneAddressErase")
        hex_to_com.Add(24, "GetSignalStrength")
        hex_to_com.Add(25, "GetNoiseStrength")
        hex_to_com.Add(26, "ReportSignalStrength")
        hex_to_com.Add(27, "ReportNoiseStrength")
        hex_to_com.Add(28, "GetAllIdPulse")
        hex_to_com.Add(29, "GetOnlyOnIdPulse")
        hex_to_com.Add(30, "ReportAllIdPulse3Phase")
        hex_to_com.Add(31, "ReportOnlyOnIdPulse3Phase")

        '---------- Creation table des acks ----------
        'table_ack.Dispose()
        'Dim x As New DataColumn
        'x = New DataColumn
        'x.ColumnName = "adresse"
        'table_ack.Columns.Add(x)
        'x = New DataColumn
        'x.ColumnName = "ordre"
        'table_ack.Columns.Add(x)
        'x = New DataColumn
        'x.ColumnName = "data1"
        'table_ack.Columns.Add(x)
        'x = New DataColumn
        'x.ColumnName = "data2"
        'table_ack.Columns.Add(x)
        'x = New DataColumn
        'x.ColumnName = "datetime"
        'table_ack.Columns.Add(x)

    End Sub

    Public Function ouvrir(ByVal numero As String) As String
        Try
            If Not port_ouvert Then
                port_name = numero
                port.PortName = numero 'nom du port : COM1
                port.BaudRate = 9600 'vitesse du port 300, 600, 1200, 2400, 9600, 14400, 19200, 38400, 57600, 115200
                port.Parity = IO.Ports.Parity.None 'pas de parité
                port.StopBits = IO.Ports.StopBits.One 'un bit d'arrêt par octet
                port.DataBits = 8 'nombre de bit par octet
                'port.Handshake = Handshake.None
                port.ReadTimeout = 3000
                port.WriteTimeout = 5000
                'port.RtsEnable = False 'ligne Rts désactivé
                'port.DtrEnable = False 'ligne Dtr désactivé
                port.Open()
                port_ouvert = True
                AddHandler port.DataReceived, New SerialDataReceivedEventHandler(AddressOf DataReceived)
                Return ("Port " & port_name & " ouvert")
            Else
                Return ("Port " & port_name & " dejà ouvert")
            End If
        Catch ex As Exception
            Return ("ERR: " & ex.Message)
        End Try
    End Function

    Public Function fermer() As String
        Try
            If port_ouvert Then
                If (Not (port Is Nothing)) Then ' The COM port exists.
                    If port.IsOpen Then
                        'vidage des tampons
                        Dim i As Integer = 0
                        port.DiscardOutBuffer()
                        Do While (port.BytesToWrite > 0 And i < 50) ' Wait for the transmit buffer to empty.
                            i = i + 1
                            domos_svc.log("PLC : Wait " & port.BytesToWrite & "BytesToWrite " & i, 9)
                            domos_svc.wait(10)
                        Loop
                        i = 0
                        port.DiscardInBuffer()
                        Do While (port.BytesToRead > 0 And i < 20) ' Wait for the receipt buffer to empty.
                            i = i + 1
                            domos_svc.log("PLC : Wait " & port.BytesToRead & "BytesToRead " & i, 9)
                            domos_svc.wait(10)
                        Loop
                        port.Close()
                        port.Dispose()
                        port_ouvert = False
                        Return ("Port " & port_name & " fermé")
                    Else
                        Return ("Port " & port_name & "  est déjà fermé")
                    End If
                Else
                    Return ("Port " & port_name & " n'existe pas")
                End If
            Else
                Return ("Port " & port_name & "  est déjà fermé (port_ouvert=false)")
            End If
        Catch ex As UnauthorizedAccessException
            Return ("ERR: Port " & port_name & " IGNORE")
            ' The port may have been removed. Ignore.
        End Try
        Return True
    End Function

    'pas utilisé 
    Public Function lire() As String
        'Return port_serie.lire()
        Return ""
    End Function

    Public Function adresse_to_hex(ByVal adresse As String)
        'convertit une adresse du type L1 en byte
        Dim table() As String = {0, 16, 32, 48, 64, 80, 96, 112, 128, 144, 160, 176, 192, 208, 224, 240}
        If adresse.Length > 1 Then
            Return table(Asc(Microsoft.VisualBasic.Left(adresse, 1)) - 65) + CInt(Microsoft.VisualBasic.Right(adresse, adresse.Length - 1)) - 1
        Else
            Return table(Asc(adresse) - 65)
        End If
    End Function

    Public Function hex_to_adresse(ByVal adresse As Byte)
        'convertit une adresse en byte en type L1
        Dim x As Integer = 0
        Dim y As Integer = 0

        If adresse >= 128 Then
            x = x + 8
            adresse = adresse - 128
        End If
        If adresse >= 64 Then
            x = x + 4
            adresse = adresse - 64
        End If
        If adresse >= 32 Then
            x = x + 2
            adresse = adresse - 32
        End If
        If adresse >= 16 Then
            x = x + 1
            adresse = adresse - 16
        End If
        If adresse >= 8 Then
            y = y + 8
            adresse = adresse - 8
        End If
        If adresse >= 4 Then
            y = y + 4
            adresse = adresse - 4
        End If
        If adresse >= 2 Then
            y = y + 2
            adresse = adresse - 2
        End If
        If adresse >= 1 Then
            y = y + 1
            adresse = adresse - 1
        End If
        Return Chr(x + 65) & (y + 1)
    End Function

    Public Function ecrire(ByVal adresse As String, ByVal commande As String, ByVal data1 As Integer, ByVal data2 As Integer) As String
        'adresse= adresse du composant : A1
        'commande : ON, OFF...
        'data1 et 2, voir description des actions plus haut ou doc plcbus
        Dim _adresse = 0
        Dim _cmd = 0
        'Dim tblack() As DataRow

        If port_ouvert Then
            Dim usercode = &HD1 'D1
            Try
                _adresse = adresse_to_hex(adresse)
            Catch ex As Exception
                domos_svc.log("PLC : Ecrire : Adresse non valide : " & _adresse, 2)
                Return ""
            End Try
            Try
                _cmd = com_to_hex(commande)
            Catch ex As Exception
                domos_svc.log("PLC : Ecrire : " & commande & " n est pas une commande valide", 2)
                Return ""
            End Try

            Try
                '--- TriPhase ---
                If domos_svc.PLC_triphase Then _cmd = _cmd Or &H40
                '--- request acks --- (sauf pour les status_request car pas important et encombre le port)
                If commande <> "STATUS_REQUEST" And commande <> "GetOnlyOnIdPulse" And commande <> "GetAllIdPulse" And commande <> "ReportAllIdPulse3Phase" And commande <> "ReportOnlyOnIdPulse3Phase" Then
                	_cmd = _cmd Or &H20
                End If
                ' --- correction data suivant la commande ---
                If commande = "ON" Then
                    data1 = 100
                    data2 = 0
                ElseIf commande = "OFF" Then
                    data1 = 0
                    data2 = 0
                End If

                Dim donnee() As Byte = {&H2, &H5, usercode, _adresse, _cmd, data1, data2, &H3}

                'ecriture sur le port
                port.Write(donnee, 0, donnee.Length)
                'port.Write(donnee, 0, donnee.Length) 'on ecrit deux fois : voir la norme PLCBUS

                'gestion des acks (sauf pour les status_request car pas important et encombre le port)
                If Not attente_ack() And commande <> "STATUS_REQUEST" And commande <> "GetOnlyOnIdPulse" And commande <> "GetAllIdPulse" And commande <> "ReportAllIdPulse3Phase" And commande <> "ReportOnlyOnIdPulse3Phase" Then
                    'pas de ack, on relance l ordre
                    domos_svc.log("PLC : Ecrire : Pas de Ack -> resend : " & adresse & " : " & commande & " " & data1 & "-" & data2, 2)
                    port.Write(donnee, 0, donnee.Length)
                    'port.Write(donnee, 0, donnee.Length) 'on ecrit deux fois : voir la norme PLCBUS
                    If Not attente_ack() Then
                        'pas de ack > NOK
                        domos_svc.log("PLC : Ecrire : Pas de Ack --> " & adresse & " : " & commande & " " & data1 & "-" & data2, 2)
                        Return "" 'on renvoi rien car le composant n'a pas recu l'ordre
                    End If
                End If
            Catch ex As Exception
                domos_svc.log("PLC : Ecrire exception : " & ex.Message & " --> " & adresse & " : " & commande & " " & data1 & "-" & data2, 2)
            End Try

            'renvoie la valeur ecrite
            Select Case UCase(commande)
                Case "ON", "ALL_LIGHTS_ON", "All_USER_LIGHTS_ON" : Return "ON"
                Case "OFF", "ALL_UNITS_OFF", "ALL_LIGHTS_OFF", "All_USER_LIGHTS_OFF", "All_USER_UNITS_OFF" : Return "OFF"
                Case "PRESET_DIM" : Return "ON" 'data1
                Case Else : Return ""
            End Select

        Else
            domos_svc.log("PLC : Ecrire : Port fermé", 2)
            Return ""
        End If

    End Function

    Private Function attente_ack() As Boolean
        'test si on recoit le ack pendant 1.5 secondes
        Dim nbtest As Integer = 0
        While nbtest < 15
            If ackreceived Then 'ack recu on sort
                ackreceived = False
                Return True
            Else
                nbtest += 1
                domos_svc.wait(10) 'on attend 0.1s
            End If
        End While
        Return False
    End Function

    Private Sub DataReceived(ByVal sender As Object, ByVal e As SerialDataReceivedEventArgs)
        'fonction qui lit les données sur le port serie
        Try
            Dim count As Integer = 0
            count = port.BytesToRead
            If port_ouvert And count > 0 Then
                port.Read(BufferIn, 0, count)
                For i As Integer = 0 To count - 1
                    ProcessReceivedChar(BufferIn(i))
                Next
            End If
        Catch Ex As Exception
            domos_svc.log("PLC : Datareceived Exception : " & Ex.Message, 2)
        End Try
    End Sub

    Private Sub ProcessReceivedChar(ByVal temp As Byte)
        'fonction qui rassemble un message complet
        'si c'est le premier byte qu'on recoit
        Try
            If firstbyte Then
                firstbyte = False
                bytecnt = 0
            End If
            recbuf(bytecnt) = temp
            bytecnt += 1
            If bytecnt = 9 Then
                firstbyte = True
                Process(recbuf)
            End If
        Catch ex As Exception
            domos_svc.log("PLC : ProcessReceivedChar Exception : " & ex.Message, 2)
        End Try
    End Sub

    Private Sub Process(ByVal comBuffer() As Byte)
        'traite le message recu
        Dim plcbus_commande As String = ""
        Dim plcbus_adresse As String = ""
        Dim data1 As String = ""
        Dim data2 As String = ""
        Dim actif As Boolean
        Dim listeactif As String = ""
        Dim ack As Boolean = False
        Dim TblBits(7) As Boolean
        Dim unbyte As Byte

        Try
            If ((comBuffer(1) = &H6) And (comBuffer(0) = &H2) And (comBuffer(8) = &H3)) Then ' si trame de reponse valide
                plcbus_adresse = hex_to_adresse(comBuffer(3))
                plcbus_commande = hex_to_com(comBuffer(4))
                data1 = comBuffer(5)
                data2 = comBuffer(6)

                'test si c'est un ack
                unbyte = comBuffer(7)
                For Iteration As Integer = 0 To 7
                    TblBits(Iteration) = unbyte And 1
                    unbyte >>= 1
                Next
                If TblBits(4) Then ack = True

                If ack Then
                    'c'est un Ack
                    ackreceived = True
                    If plcbus_commande = "STATUS_REQUEST" Or plcbus_commande <> "GetOnlyOnIdPulse" Or plcbus_commande <> "GetAllIdPulse" Then
                        domos_svc.log("DBG: " & "PLC : <- ACK :" & plcbus_commande & "-" & plcbus_adresse & " : " & data1 & "-" & data2 & " : " & comBuffer(7), 10)
                    Else
                        domos_svc.log("PLC : <- ACK :" & plcbus_commande & "-" & plcbus_adresse & " : " & data1 & "-" & data2 & " : " & comBuffer(7), 9)
                    End If
                Else
                    'ce n'est pas un ack, je traite le paquet
                    ackreceived = False

                    Select Case plcbus_commande
                        Case "ON", "OFF", "DIM", "BRIGHT", "PRESET_DIM", "BLINK", "FADE_STOP", "STATUS_REQUEST"
                            If data1 = 0 Then traitement("OFF", plcbus_adresse, plcbus_commande) Else traitement("ON", plcbus_adresse, plcbus_commande)
                        Case "GetSignalStrength", "GetNoiseStrength"
                            domos_svc.log("PLC : <- " & plcbus_commande & "-" & plcbus_adresse & " : " & data1, 9)
                        Case "ReceiverMasterAddressSetup", "TransmitterMasterAddressSetup"
                            domos_svc.log("PLC : <- " & plcbus_commande & "-" & plcbus_adresse & " : " & data1 & "-" & data2, 9)
                        Case "ALL_UNITS_OFF", "All_USER_UNITS_OFF"
                            domos_svc.log("PLC : <- " & plcbus_commande & "-" & plcbus_adresse & " : " & data1 & "-" & data2, 9)
                        Case "ALL_LIGHTS_ON", "ALL_LIGHTS_OFF", "All_USER_LIGHTS_ON", "All_USER_LIGHTS_OFF"
                            domos_svc.log("PLC : <- " & plcbus_commande & "-" & plcbus_adresse & " : " & data1 & "-" & data2, 9)
                        Case "STATUS_ON"
                            traitement("ON", plcbus_adresse, plcbus_commande)
                        Case "STATUS_OFF"
                            traitement("OFF", plcbus_adresse, plcbus_commande)
                        Case "SceneAddressSetup", "SceneAddressErase", "AllSceneAddressErase"
                            domos_svc.log("PLC : <- " & plcbus_commande & "-" & plcbus_adresse & " : " & data1 & "-" & data2, 9)
                        Case "ReportSignalStrength", "ReportNoiseStrength"
                            domos_svc.log("PLC : <- " & plcbus_commande & "-" & plcbus_adresse & " : " & data1 & "-" & data2, 9)
                        Case "GetAllIdPulse", "ReportAllIdPulse3Phase"
                            unbyte = comBuffer(6)
                            For Iteration As Integer = 0 To 7
                                actif = (unbyte And 1)
                                If actif Then listeactif = listeactif & " " & Left(plcbus_adresse, 1) & (Iteration + 1)
                                unbyte >>= 1
                            Next
                            unbyte = comBuffer(5)
                            For Iteration As Integer = 0 To 7
                                actif = (unbyte And 1)
                                If actif Then listeactif = listeactif & " " & Left(plcbus_adresse, 1) & (Iteration + 9)
                                unbyte >>= 1
                            Next
                            domos_svc.log("PLC : <- Liste des composants actifs : " & listeactif, 9)
                        Case "GetOnlyOnIdPulse", "ReportOnlyOnIdPulse3Phase"
                            unbyte = comBuffer(6)
                            For Iteration As Integer = 0 To 7
                                actif = (unbyte And 1)
                                If actif Then
                                    listeactif = listeactif & " " & Left(plcbus_adresse, 1) & (Iteration + 1)
                                    traitement("ON", Left(plcbus_adresse, 1) & (Iteration + 1), "GetOnlyOnIdPulse")
                                Else
                                    traitement("OFF", Left(plcbus_adresse, 1) & (Iteration + 1), "GetOnlyOnIdPulse")
                                End If
                                unbyte >>= 1
                            Next
                            unbyte = comBuffer(5)
                            For Iteration As Integer = 0 To 7
                                actif = (unbyte And 1)
                                If actif Then
                                    listeactif = listeactif & " " & Left(plcbus_adresse, 1) & (Iteration + 9)
                                    traitement("ON", Left(plcbus_adresse, 1) & (Iteration + 9), "GetOnlyOnIdPulse")
                                Else
                                    traitement("OFF", Left(plcbus_adresse, 1) & (Iteration + 9), "GetOnlyOnIdPulse")
                                End If
                                unbyte >>= 1
                            Next
                            domos_svc.log("DBG: PLC : <- Liste des composants ON : " & Left(plcbus_adresse, 1) & " -> " & listeactif, 10)
                    End Select


                End If
            Else
                domos_svc.log("PLC : Process : Message recu invalide", 2)
            End If
        Catch ex As Exception
            domos_svc.log("PLC : Process Exception : " & ex.Message, 2)
        End Try
    End Sub

    Private Sub traitement(ByVal valeurretour As String, ByVal plcbus_adresse As String, ByVal plcbus_commande As String)
        If valeurretour <> "" Then
            Try
                Dim tabletmp() As DataRow
                tabletmp = domos_svc.table_composants.Select("composants_adresse = '" & plcbus_adresse & "'")
                If tabletmp.GetUpperBound(0) >= 0 Then
                    '--- comparaison du relevé avec le dernier etat ---
                    Dim x As String = tabletmp(0)("composants_etat").ToString()
                    If valeurretour <> x Then
                        domos_svc.log("PLC : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeurretour, 6)
                        '  --- modification de l'etat du composant dans la table en memoire ---
                        tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
                        tabletmp(0)("composants_etat") = valeurretour
                        tabletmp(0)("composants_etatdate") = DateAndTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    Else
                        domos_svc.log("PLC : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeurretour & " (inchangé)", 7)
                        '--- Modification de la date dans la base SQL ---
                        Dim Err As String = domos_svc.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & DateAndTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                        If Err <> "" Then domos_svc.log("PLC: Process : mysql error : " & Err, 2)
                    End If
                Else
                    If plcbus_commande <> "GetOnlyOnIdPulse" Then
                        domos_svc.log("PLC : Process : Pas de composant pour cette adresse : " & plcbus_adresse & " : " & valeurretour, 2)
                    End If
                End If
            Catch ex As Exception
                domos_svc.log("PLC : traitement Exception : " & ex.Message & " --> " & plcbus_adresse & " : " & plcbus_commande & "-" & valeurretour, 2)
            End Try
        End If
    End Sub

    'Private Sub DataReceived(ByVal sender As Object, ByVal e As SerialDataReceivedEventArgs)
    '    Dim reponse As String = ""
    '    Dim err As String = ""
    '    Dim dateheure As String
    '    Dim bytes As Integer
    '    Dim ack As Boolean = False
    '    'create a byte array to hold the awaiting data
    '    bytes = port.BytesToRead
    '    Dim comBuffer As Byte() = New Byte(bytes - 1) {}
    '    While True
    '        If port_ouvert Then
    '            Try
    '                bytes = port.BytesToRead
    '            Catch ex As Exception
    '            End Try
    '            If bytes = 9 Then
    '                comBuffer = New Byte(bytes - 1) {}
    '                port.Read(comBuffer, 0, bytes)
    '                If ((comBuffer(1) = &H6) And (comBuffer(0) = &H2) And (comBuffer(8) = &H3)) Then ' si trame de reponse valide
    '                    'For Each data As Byte In comBuffer
    '                    '    reponse = reponse & (Convert.ToString(data, 16).PadLeft(2, "0"c).PadRight(3, " "c))
    '                    'Next
    '                    'traitement de la trame recu
    '                    Dim plcbus_commande As String = hex_to_com(comBuffer(4))
    '                    Dim plcbus_adresse As String = hex_to_adresse(comBuffer(3))
    '                    Dim data1 As String = comBuffer(5)
    '                    Dim data2 As String = comBuffer(6)
    '                    'If CInt(comBuffer(7) & &H20) Mod 32 Then ack = True

    '                    'test si c'est un ack
    '                    Dim TblBits(7) As Boolean
    '                    Dim xx As Byte = comBuffer(7)
    '                    For Iteration As Integer = 0 To 7 'Boucle
    '                        TblBits(Iteration) = xx And 1
    '                        xx >>= 1
    '                    Next
    '                    If TblBits(4) Then ack = True

    '                    'If Not ack Then Domos.log(reponse)
    '                    If ack Then
    '                        ackreceived = True 'c'est un Ack
    '                        reponse = "PLC : <- ACK :" & plcbus_commande & "-" & plcbus_adresse & " : " & data1 & "-" & data2 & " : " & comBuffer(7)
    '                        If plcbus_commande = "STATUS_REQUEST" Then
    '                            domos_svc.log("DBG: " & reponse, 10)
    '                        Else
    '                            domos_svc.log(reponse, 9)
    '                        End If

    '                    Else
    '                        'ce n'est pas un ack, je traite le paquet
    '                        ackreceived = False
    '                        Dim valeur As String = data1
    '                        Dim tabletmp() As DataRow
    '                        Try
    '                            tabletmp = domos_svc.table_composants.Select("composants_adresse = '" & plcbus_adresse & "'")
    '                            If tabletmp.GetUpperBound(0) >= 0 Then
    '                                'conversion de la valeur de 0 à 100 en ON/OFF
    '                                If valeur = 0 Then
    '                                    valeur = "OFF"
    '                                Else
    '                                    valeur = "ON"
    '                                End If
    '                                '--- comparaison du relevé avec le dernier etat ---
    '                                Dim x As String = tabletmp(0)("composants_etat").ToString()
    '                                If valeur <> x Then
    '                                    domos_svc.log("PLC : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur, 6)
    '                                    '  --- modification de l'etat du composant dans la table en memoire ---
    '                                    tabletmp(0)("composants_etat") = valeur
    '                                    tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
    '                                Else
    '                                    domos_svc.log("PLC : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur & " (inchangé)", 7)
    '                                    '--- Modification de la date dans la base SQL ---
    '                                    dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
    '                                    err = domos_svc.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
    '                                    If err <> "" Then domos_svc.log("PLC: inchange precision " & err, 8)
    '                                End If
    '                            Else
    '                                domos_svc.log("PLC : Pas de composant pour cette adresse : " & plcbus_adresse & " : " & valeur, 2)
    '                            End If
    '                        Catch ex As Exception
    '                            domos_svc.log("PLC : ERROR acces table_composants : " & ex.Message, 2)
    '                        End Try

    '                    End If
    '                    Exit While
    '                End If
    '            End If
    '        End If

    '    End While
    'End Sub

End Class
