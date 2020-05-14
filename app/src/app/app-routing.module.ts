import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { MarketsComponent } from './markets/markets.component';
import { PortfolioComponent } from './portfolio/portfolio.component';
import { NewsComponent } from './news/news.component';


const routes: Routes = [
  { path: 'dashboard', component: DashboardComponent },
  { path: 'portfolio', component: PortfolioComponent },
  { path: 'markets', component: MarketsComponent },
  { path: 'news', component: NewsComponent },
  { path: '',   redirectTo: '/dashboard', pathMatch: 'full' }, // redirect to `dashboard`
  { path: '**', component: DashboardComponent },
]

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
