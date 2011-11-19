Imports System.Threading
Imports System.IO

Public Class bspOptimizer
#Const UseOptimizer = 0

    Private Shared _Cancel As Boolean = False
    Private _Optimize As New AutoResetEvent(False)
    Private _optimizationlist As New SortedList(Of String, BspRect)(250000)

    Public Sub New()


#If UseOptimizer = 1 Then
        Deserialize()

        Dim th As New Thread(AddressOf Optimize)
        th.Start()
#End If

    End Sub

    Public Shared Sub Terminate()
        _Cancel = True

    End Sub

    Private Function GetBspRectKey(ByVal r As BspRect) As String
        Return ChrW(AscW("Z"c) - r.Z) & "_" & r.P1.ToString
    End Function

    Private Sub Optimize(ByVal state As Object)

        Return
        'Dim rect As BspRect

        'While Not _Cancel

        '    If _Optimize.WaitOne(500, False) Then

        '        SyncLock _optimizationlist
        '            rect = _optimizationlist(_optimizationlist.Keys(0))
        '            _optimizationlist.RemoveAt(0)
        '            If _optimizationlist.Count > 0 Then
        '                _Optimize.Set()
        '            End If
        '        End SyncLock

        '        Dim HasValue As Boolean = False

        '        For Each subrect In rect.SubRects
        '            If subrect IsNot Nothing Then
        '                HasValue = True
        '                Exit For
        '            End If
        '        Next

        '        If HasValue Then
        '            For Each subrect In rect.SubRects
        '                Try
        '                    If subrect.InLand = BspRect.inlandstate.Unknown Then
        '                        Dim center As New Coords
        '                        center.Lat = (subrect.P1.Lat + subrect.P2.Lat) / 2
        '                        center.Lon = (subrect.P1.Lon + subrect.P2.Lon) / 2
        '                        GSHHS_Reader.HitTest(center, 0, GSHHS_Reader.Polygons(center), True)
        '                    End If

        '                Catch
        '                End Try

        '            Next
        '        End If
        '    End If


        'End While

        'Serialize()

    End Sub


    Public Sub AddToOptimizationList(ByVal r As BspRect)

        SyncLock _optimizationlist
            Dim Key As String = GetBspRectKey(r)
            If _optimizationlist.Count < 250000 AndAlso Not _optimizationlist.ContainsKey(Key) Then
                _optimizationlist.Add(Key, r)
                _Optimize.Set()
            End If

        End SyncLock

    End Sub

    Private ReadOnly Property BSPFileName() As String
        Get
            Dim Folder As String = Path.Combine(RouteurModel.BaseFileDir, "BSP")

            If Not Directory.Exists(Folder) Then
                Directory.CreateDirectory(Folder)
            End If

            Return Path.Combine(Folder, "BSP_tree.xml")
        End Get
    End Property

    Private Sub Deserialize()

        Dim sr As New Xml.Serialization.XmlSerializer(GetType(BspRect))

        If File.Exists(BSPFileName) Then
            Using stream As New StreamReader(BSPFileName)
                GSHHS_Reader._Tree = CType(sr.Deserialize(stream), BspRect)
            End Using
#If BSP_STATS Then
            BspRect.BspCount = GSHHS_Reader._Tree.NodeCount
            Stats.SetStatValue(Stats.StatID.BSPCellCount) = CDbl(BspRect.BspCount)
            Console.WriteLine(GSHHS_Reader._Tree.UnknownNodeCount)
#End If
        End If

    End Sub

    Private Sub Serialize()

#If UseOptimizer Then
        Dim sr As New Xml.Serialization.XmlSerializer(GetType(BspRect))

        sr.Serialize(New StreamWriter(BspFileName), GSHHS_Reader._Tree)
#End If
    End Sub

End Class
