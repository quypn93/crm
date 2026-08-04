import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { OrderCostsComponent } from './order-costs/order-costs.component';
import { PayrollComponent } from './payroll/payroll.component';
import { FixedExpensesComponent } from './fixed-expenses/fixed-expenses.component';
import { ExpenseCategoriesComponent } from './expense-categories/expense-categories.component';
import { ProfitReportComponent } from './profit-report/profit-report.component';

const routes: Routes = [
  { path: '', redirectTo: 'order-costs', pathMatch: 'full' },
  { path: 'order-costs', component: OrderCostsComponent },
  { path: 'payroll', component: PayrollComponent },
  { path: 'fixed-expenses', component: FixedExpensesComponent },
  { path: 'expense-categories', component: ExpenseCategoriesComponent },
  { path: 'profit', component: ProfitReportComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class FinanceRoutingModule { }
