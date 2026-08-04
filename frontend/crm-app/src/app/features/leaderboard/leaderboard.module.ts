import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { LeaderboardRoutingModule } from './leaderboard-routing.module';
import { LeaderboardComponent } from './leaderboard/leaderboard.component';

@NgModule({
  declarations: [
    LeaderboardComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    LeaderboardRoutingModule
  ]
})
export class LeaderboardModule { }
