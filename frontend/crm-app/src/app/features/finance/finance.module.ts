import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { FinanceRoutingModule } from './finance-routing.module';
import { OrderCostsComponent } from './order-costs/order-costs.component';
import { PayrollComponent } from './payroll/payroll.component';
import { FixedExpensesComponent } from './fixed-expenses/fixed-expenses.component';
import { ExpenseCategoriesComponent } from './expense-categories/expense-categories.component';
import { ProfitReportComponent } from './profit-report/profit-report.component';

@NgModule({
  declarations: [
    OrderCostsComponent,
    PayrollComponent,
    FixedExpensesComponent,
    ExpenseCategoriesComponent,
    ProfitReportComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    FinanceRoutingModule
  ]
})
export class FinanceModule { }
