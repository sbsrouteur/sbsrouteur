'This file is part of Routeur.
'Copyright (C) 2010-2013  sbsRouteur(at)free.fr

'Routeur is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'Routeur is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Math

Module VBVMGNavHelper

    Public Function ComputeTrackVBVMG(mi As MeteoInfo, Sails As clsSailManager, BoatType As String, Start As Coords, Dest1 As Coords, Dest2 As Coords, ByRef BoatSpeed As Double, ByRef ReachedWP As Boolean) As Coords

        If mi Is Nothing Then
            Return Nothing
        End If
        Dim TC As New TravelCalculator
        Dim Dest As Coords
        Dim RetP As Coords

        TC.StartPoint = Start
        If Dest2 Is Nothing Then
            Dest = Dest1
        Else
            Dest = GSHHS_Reader.PointToSegmentIntersect(Start, Dest1, Dest2)
        End If
        TC.EndPoint = Dest
        Dim CapOrtho As Double = TC.OrthoCourse_Deg
        Dim Angle As Double
        BoatSpeed = 0

        Dim WPDist As Double = TC.SurfaceDistance

        Angle = do_vbvmg(Start, Dest, mi, Sails, BoatType)

        BoatSpeed = Sails.GetSpeed(BoatType, clsSailManager.EnumSail.OneSail, VLM_Router.WindAngle(Angle, mi.Dir), mi.Strength)

        ReachedWP = WPDist <= BoatSpeed / 60 * RouteurModel.VacationMinutes
             
        RetP = TC.ReachDistanceortho(BoatSpeed / 60 * RouteurModel.VacationMinutes, GribManager.CheckAngleInterp(Angle))


        TC.StartPoint = Nothing
        TC.EndPoint = Nothing
        TC = Nothing
        Return RetP

    End Function


    Public Function do_vbvmg(ByVal Start As Coords, ByVal Dest As Coords, ByVal mi As MeteoInfo, Sails As clsSailManager, BoatType As String) As Double

        '254	  double alpha, beta;
        '255	  double speed, speed_t1, speed_t2, l1, l2, d1, d2;
        '256	  double angle, maxangle, t, t1, t2, t_min;
        Dim t_min As Double
        '257	  double wanted_heading;
        '258	  double w_speed, w_angle;
        '259	  double dist, tanalpha, d1hypotratio;
        '260	  double b_alpha, b_beta, b_t1, b_t2, b_l1, b_l2;
        '261	  double b1_alpha, b1_beta;
        '262	  double speed_alpha, speed_beta;
        '263	  double vmg_alpha, vmg_beta;
        '264	  int i,j, min_i, min_j, max_i, max_j;
        Dim i As Integer
        Dim j As Integer
        Dim alpha As Double
        Dim TanAlpha As Double
        Dim TanBeta As Double
        Dim beta As Double
        Dim D1HypotRatio As Double
        Dim SpeedT1 As Double
        Dim SpeedT2 As Double
        Dim ISigne As Integer = 1
        Dim D1 As Double
        Dim D2 As Double
        Dim L1 As Double
        Dim L2 As Double
        Dim T As Double
        Dim T1 As Double
        Dim T2 As Double
        Dim b_Alpha As Double
        Dim b_Beta As Double
        Dim b_L1 As Double
        Dim b_L2 As Double
        Dim b_T1 As Double
        Dim b_T2 As Double
        Dim SpeedAlpha As Double
        Dim SpeedBeta As Double
        Dim VMGAlpha As Double
        Dim VMGBeta As Double
        Dim angle As Double




        '265:
        '266	  b_t1 = b_t2 = b_l1 = b_l2 = b_alpha = b_beta = beta = 0.0;
        '267:
        Dim Tc As New TravelCalculator With {.StartPoint = Start, .EndPoint = Dest}
        Dim Dist As Double = Tc.SurfaceDistance
        Dim CapOrtho As Double = Tc.OrthoCourse_Deg
        '268	  dist = ortho_distance(aboat->latitude, aboat->longitude,
        '269	                        aboat->wp_latitude, aboat->wp_longitude);
        '270:

        '271	  get_wind_info_context(context, aboat, &aboat->wind);
        '272	  set_heading_ortho_nowind(aboat);
        '273:
        '274	  wanted_heading = aboat->heading;
        '275	  maxangle = wanted_heading;
        '276:
        '277	  w_speed = aboat->wind.speed;
        '278	  w_angle = aboat->wind.angle;
        '279:
        '280	  /* first compute the time for the "ortho" heading */
        '281	  speed = find_speed(aboat, w_speed, w_angle - wanted_heading);
        Dim Speed As Double = Sails.GetSpeed(BoatType, clsSailManager.EnumSail.OneSail, VLM_Router.WindAngle(CapOrtho, mi.Dir), mi.Strength)
        If Speed > 0 Then
            t_min = Dist / Speed
        Else
            t_min = 365 * 24
        End If
        '282	  if (speed > 0.0) {
        '283	    t_min = dist / speed;
        '284	  } else {
        '285	    t_min = 365.0*24.0; /* one year :) */
        '286	  }
        '287:
        '288:#If DEBUG Then
        '289	  printf("VBVMG: Wind %.2fkts %.2f\n", w_speed, radToDeg(w_angle));
        '290	  printf("VBVMG Direct road: heading %.2f time %.2f\n", 
        '291	         radToDeg(wanted_heading), t_min);
        '292	  printf("VBVMG Direct road: wind angle %.2f\n", 
        '293	         radToDeg(w_angle-wanted_heading));
        '294	#endif /* DEBUG */
        '295	
        angle = mi.Dir - CapOrtho

        If angle < -90 Then
            angle += 360
        ElseIf angle > 90 Then
            angle -= 360
        End If
        '296	  angle = w_angle - wanted_heading;
        '297	  if (angle < -PI ) {
        '298	    angle += TWO_PI;
        '299	  } else if (angle > PI) {
        '300	    angle -= TWO_PI;
        '301	  }
        If angle > 0 Then
            ISigne = -1
            '302	  if (angle < 0.0) {
            '303	    min_i = 1;
            '304	    min_j = -89;
            '305	    max_i = 90;
            '306	    max_j = 0;
            '307	  } else {
            '308	    min_i = -89;
            '309	    min_j = 1;
            '310	    max_i = 0;
            '311	    max_j = 90;
            '312	  }
        End If
        '313	
        '314	  for (i=min_i; i<max_i; i++) {
        For i = 1 To 90
            alpha = i * Math.PI / 180
            TanAlpha = Tan(alpha)
            D1HypotRatio = Sqrt(1 + TanAlpha * TanAlpha)
            '315	    alpha = degToRad((double)i);
            '316	    tanalpha = tan(alpha);
            '317	    d1hypotratio = hypot(1, tan(alpha));
            '318	    speed_t1 = find_speed(aboat, w_speed, angle-alpha);
            SpeedT1 = Sails.GetSpeed(BoatType, clsSailManager.EnumSail.OneSail, VLM_Router.WindAngle(CapOrtho - i * ISigne, mi.Dir), mi.Strength)
            '319	    if (speed_t1 <= 0.0) {
            '320	      continue;
            '321	    }
            If SpeedT1 > 0 Then

                For j = -89 To 0
                    beta = j * PI / 180
                    D1 = Dist * (Tan(-beta) / (TanAlpha + Tan(-beta)))
                    L1 = D1 * D1HypotRatio
                    '322	    for (j=min_j; j<max_j; j++) {
                    '323	      beta = degToRad((double)j);
                    '324	      d1 = dist * (tan(-beta) / (tanalpha + tan(-beta)));
                    '325	      l1 =  d1 * d1hypotratio;
                    '326	      t1 = l1 / speed_t1;
                    T1 = L1 / SpeedT1
                    If T1 < 0 OrElse T1 > t_min Then
                        Continue For
                    End If
                    '327	      if ((t1 < 0.0) || (t1 > t_min)) {
                    '328	        continue;
                    '329	      }
                    D2 = Dist - D1
                    '330	      d2 = dist - d1; 
                    SpeedT2 = Sails.GetSpeed(BoatType, clsSailManager.EnumSail.OneSail, VLM_Router.WindAngle(CapOrtho - j * ISigne, mi.Dir), mi.Strength)
                    '331	      speed_t2 = find_speed(aboat, w_speed, angle-beta);
                    If SpeedT2 <= 0 Then
                        Continue For
                    End If
                    '332	      if (speed_t2 <= 0.0) {
                    '333	        continue;
                    '334	      }
                    '335	      l2 =  d2 * hypot(1, tan(-beta));
                    TanBeta = Tan(-beta)
                    L2 = D2 * Sqrt(1 + TanBeta * TanBeta)
                    '336	      t2 = l2 / speed_t2;
                    T2 = L2 / SpeedT2
                    '337	      if (t2 < 0.0) {
                    '338	        continue;
                    '339	      }

                    T = T1 + T2
                    If T < t_min Then
                        '340	      t = t1 + t2;
                        '341	      if (t < t_min) {
                        '342	        t_min = t;
                        '343	        b_alpha = alpha;
                        '344	        b_beta  = beta;
                        '345	        b_l1 = l1;
                        '346	        b_l2 = l2;
                        '347	        b_t1 = t1;
                        '348	        b_t2 = t2;
                        t_min = T
                        b_Alpha = i
                        b_Beta = j
                        b_L1 = L1
                        b_L2 = L2
                        b_T1 = T1
                        b_T2 = T2
                        '349	      }
                        '350	    }
                    End If
                Next
            End If
            '351	  }
        Next
        '352	#if DEBUG
        '353	  printf("VBVMG: alpha=%.2f, beta=%.2f\n", radToDeg(b_alpha), radToDeg(b_beta));
        '354	#endif /* DEBUG */
        '355	  if (mode) {
        '356	    b1_alpha = b_alpha;
        '357	    b1_beta = b_beta;
        '358	    for (i=-9; i<=9; i++) {
        '359	      alpha = b1_alpha + degToRad(((double)i)/10.0);
        '360	      tanalpha = tan(alpha);
        '361	      d1hypotratio = hypot(1, tan(alpha));
        '362	      speed_t1 = find_speed(aboat, w_speed, angle-alpha);
        '363	      if (speed_t1 <= 0.0) {
        '364	        continue;
        '365	      }
        '366	      for (j=-9; j<=9; j++) {
        '367	        beta = b1_beta + degToRad(((double)j)/10.0);
        '368	        d1 = dist * (tan(-beta) / (tanalpha + tan(-beta)));
        '369	        l1 =  d1 * d1hypotratio;
        '370	        t1 = l1 / speed_t1;
        '371	        if ((t1 < 0.0) || (t1 > t_min)) {
        '372	          continue;
        '373	        }
        '374	        d2 = dist - d1; 
        '375	        speed_t2 = find_speed(aboat, w_speed, angle-beta);
        '376	        if (speed_t2 <= 0) {
        '377	          continue;
        '378	        }
        '379	        l2 =  d2 * hypot(1, tan(-beta));
        '380	        t2 = l2 / speed_t2;
        '381	        if (t2 < 0.0) {
        '382	          continue;
        '383	        }
        '384	        t = t1 + t2;
        '385	        if (t < t_min) {
        '386	          t_min = t;
        '387	          b_alpha = alpha;
        '388	          b_beta  = beta;
        '389	          b_l1 = l1;
        '390	          b_l2 = l2;
        '391	          b_t1 = t1;
        '392	          b_t2 = t2;
        '393	        }
        '394	      }    
        '395	    }
        '396	#if DEBUG
        '397	    printf("VBVMG: alpha=%.2f, beta=%.2f\n", radToDeg(b_alpha), 
        '398	           radToDeg(b_beta));
        '399	#endif /* DEBUG */
        '400	  }
        SpeedAlpha = Sails.GetSpeed(BoatType, clsSailManager.EnumSail.OneSail, VLM_Router.WindAngle(CapOrtho - b_Alpha * ISigne, mi.Dir), mi.Strength)
        SpeedBeta = Sails.GetSpeed(BoatType, clsSailManager.EnumSail.OneSail, VLM_Router.WindAngle(CapOrtho - b_Beta * ISigne, mi.Dir), mi.Strength)
        VMGAlpha = SpeedAlpha * Cos(b_Alpha * PI / 180)
        VMGBeta = SpeedBeta * Cos(b_Beta * PI / 180)

        If VMGAlpha > VMGBeta Then
            Return CapOrtho - b_Alpha * ISigne
        Else
            Return CapOrtho - b_Beta * ISigne
        End If
        '401	  speed_alpha = find_speed(aboat, w_speed, angle-b_alpha);
        '402	  vmg_alpha = speed_alpha * cos(b_alpha);
        '403	  speed_beta = find_speed(aboat, w_speed, angle-b_beta);
        '404	  vmg_beta = speed_beta * cos(b_beta);
        '405	
        '406	#if DEBUG
        '407	    printf("VBVMG: speedalpha=%.2f, speedbeta=%.2f\n", speed_alpha, speed_beta);
        '408	    printf("VBVMG: vmgalpha=%.2f, vmgbeta=%.2f\n", vmg_alpha, vmg_beta);
        '409	    printf("VBVMG: headingalpha %.2f, headingbeta=%.2f\n",
        '410	           radToDeg(fmod(wanted_heading + b_alpha, TWO_PI)),
        '411	           radToDeg(fmod(wanted_heading + b_beta, TWO_PI)));
        '412	#endif /* DEBUG */
        '413	
        '414	  if (vmg_alpha > vmg_beta) {
        '415	    *heading1 = fmod(wanted_heading + b_alpha, TWO_PI);
        '416	    *heading2 = fmod(wanted_heading + b_beta, TWO_PI);
        '417	    *time1 = b_t1;
        '418	    *time2 = b_t2;
        '419	    *dist1 = b_l1;
        '420	    *dist2 = b_l2;
        '421	  } else {
        '422	    *heading2 = fmod(wanted_heading + b_alpha, TWO_PI);
        '423	    *heading1 = fmod(wanted_heading + b_beta, TWO_PI);
        '424	    *time2 = b_t1;
        '425	    *time1 = b_t2;
        '426	    *dist2 = b_l1;
        '427	    *dist1 = b_l2;
        '428	  }
        '429	  if (*heading1 < 0 ) {
        '430	    *heading1 += TWO_PI;
        '431	  }
        '432	  if (*heading2 < 0 ) {
        '433	    *heading2 += TWO_PI;
        '434	  }
        '435	    
        '436	  *wangle1 = fmod(*heading1 - w_angle, TWO_PI);
        '437	  if (*wangle1 > PI ) {
        '438	    *wangle1 -= TWO_PI;
        '439	  } else if (*wangle1 < -PI ) {
        '440	    *wangle1 += TWO_PI;
        '441	  }
        '442	  *wangle2 = fmod(*heading2 - w_angle, TWO_PI);
        '443	  if (*wangle2 > PI ) {
        '444	    *wangle2 -= TWO_PI;
        '445	  } else if (*wangle2 < -PI ) {
        '446	    *wangle2 += TWO_PI;
        '447	  }
        '448	#if DEBUG
        '449	  printf("VBVMG: wangle1=%.2f, wangle2=%.2f\n", radToDeg(*wangle1),
        '450	         radToDeg(*wangle2));
        '451	  printf("VBVMG: heading1 %.2f, heading2=%.2f\n", radToDeg(*heading1),
        '452	         radToDeg(*heading2));
        '453	  printf("VBVMG: dist=%.2f, l1=%.2f, l2=%.2f, ratio=%.2f\n", dist, *dist1,
        '454	         *dist2, (b_l1+b_l2)/dist);
        '455	  printf("VBVMG: t1 = %.2f, t2=%.2f, total=%.2f\n", *time1, *time2, t_min);
        '456	  printf("VBVMG: heading %.2f\n", radToDeg(*heading1));
        '457	  printf("VBVMG: wind angle %.2f\n", radToDeg(*wangle1));
        '458	#endif /* DEBUG */
        '459	}

    End Function


End Module
