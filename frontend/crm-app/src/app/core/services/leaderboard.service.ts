import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from './api.service';
import { LeaderboardResult, LeaderboardScope, LeaderboardPeriod } from '../models/leaderboard.model';

@Injectable({ providedIn: 'root' })
export class LeaderboardService {
  private readonly base = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getLeaderboard(scope: LeaderboardScope, period: LeaderboardPeriod, date?: string): Observable<LeaderboardResult> {
    let url = `${this.base}/leaderboard?scope=${scope}&period=${period}`;
    if (date) url += `&date=${date}`;
    return this.http.get<ApiResponse<LeaderboardResult>>(url).pipe(map(r => r.data));
  }
}
