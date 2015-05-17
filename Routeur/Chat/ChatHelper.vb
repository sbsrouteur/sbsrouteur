Imports agsXMPP

Module ChatHelper

    Sub AddMessage(RtfBox As RichTextBox, e As protocol.client.Message)
        Dim Stamp As DateTime
        If e.XDelay IsNot Nothing Then
            Stamp = e.XDelay.Stamp
        Else
            Stamp = Now
        End If
        AddMessage(RtfBox, e.From, e.Body, e.Type = protocol.client.MessageType.groupchat, Stamp)
    End Sub

    Public Sub AddMessage(RtfBox As RichTextBox, From As Jid, Text As String, isgroupChat As Boolean, TimeStamp As DateTime)

        Dim P As New Paragraph
        Dim FromString As String
        Dim TimeString As String

        If Now.Subtract(TimeStamp).TotalHours < 24 Then
            TimeString = TimeStamp.ToShortTimeString
        Else
            TimeString = TimeStamp.ToString
        End If

        If isgroupChat Then
            If From.Resource IsNot Nothing Then
                FromString = From.Resource
            Else
                FromString = From.User
            End If
        Else
            FromString = From.User
        End If

        P.Inlines.Add(New Run(TimeString & "<" & FromString & "> " & Text))
        RtfBox.Document.Blocks.Add(P)
        RtfBox.ScrollToEnd()

    End Sub

End Module
