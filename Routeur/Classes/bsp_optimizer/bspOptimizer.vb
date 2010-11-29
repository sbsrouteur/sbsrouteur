Imports System.Threading

Public Class bspOptimizer

    Private Shared _Cancel As Boolean = False
    Private _Optimize As New AutoResetEvent(False)
    Private _optimizationlist As New List(Of BspRect)(250000)

    Public Sub New()

        Dim th As New Thread(AddressOf Optimize)
        'th.Start()

    End Sub

    Public Shared Sub Terminate()
        _Cancel = True

    End Sub

    Private Sub Optimize(ByVal state As Object)

        Dim rect As BspRect

        While Not _Cancel

            If _Optimize.WaitOne(500, False) Then

                SyncLock _optimizationlist
                    rect = _optimizationlist(0)
                    _optimizationlist.RemoveAt(0)
                    If _optimizationlist.Count > 0 Then
                        _Optimize.Set()
                    End If
                End SyncLock

                Dim HasValue As Boolean = False

                For Each subrect In rect.SubRects
                    If subrect IsNot Nothing Then
                        HasValue = True
                        Exit For
                    End If
                Next

                If HasValue Then
                    For Each subrect In rect.SubRects
                        Try
                            If subrect.InLand = BspRect.inlandstate.Unknown Then
                                Dim center As New Coords
                                center.Lat = (subrect.P1.Lat + subrect.P2.Lat) / 2
                                center.Lon = (subrect.P1.Lon + subrect.P2.Lon) / 2
                                GSHHS_Reader.HitTest(center, 0, GSHHS_Reader.Polygons(center), True)
                            End If

                        Catch
                        End Try

                    Next
                End If
            End If


        End While

    End Sub

    Public Sub AddToOptimizationList(ByVal r As BspRect)

        SyncLock _optimizationlist

            If _optimizationlist.Count < 250000 Then
                _optimizationlist.Add(r)
                _Optimize.Set()
            End If

        End SyncLock

    End Sub


End Class
