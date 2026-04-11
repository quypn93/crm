import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { OrderService } from '../../../core/services/order.service';
import { CustomerService, Customer } from '../../../core/services/customer.service';
import { DealService, Deal } from '../../../core/services/deal.service';
import { DesignService, ColorFabric, ShirtComponent, ComponentType } from '../../../core/services/design.service';
import { UserManagementService, UserListItem } from '../../../core/services/user-management.service';
import { AuthService } from '../../../core/services/auth.service';
import { CreateOrderItemRequest } from '../../../core/models/order.model';

@Component({
  selector: 'app-order-form',
  templateUrl: './order-form.component.html',
  styleUrls: ['./order-form.component.scss']
})
export class OrderFormComponent implements OnInit {
  orderForm: FormGroup;
  isLoading = false;
  isEditMode = false;
  orderId: string | null = null;
  errorMessage = '';

  customers: Customer[] = [];
  deals: Deal[] = [];
  filteredDeals: Deal[] = [];
  filteredCustomers: Customer[] = [];
  colorFabrics: ColorFabric[] = [];
  materials: ShirtComponent[] = [];
  users: UserListItem[] = [];
  designers: UserListItem[] = [];
  customerSearchText = '';
  showCustomerDropdown = false;

  // Size grid
  readonly SIZE_LIST = ['S', 'M', 'L', 'XL', 'XXL', 'NC1', 'NC2', 'NC3'];
  sizeQty: Record<string, number> = {};

  getSizeQtyInput(size: string): number { return this.sizeQty[size] || 0; }
  onSizeQtyChange(size: string, event: Event): void {
    const val = parseInt((event.target as HTMLInputElement).value, 10);
    this.sizeQty[size] = isNaN(val) || val < 0 ? 0 : val;
  }
  getTotalQtyInput(): number {
    return Object.values(this.sizeQty).reduce((a, b) => a + b, 0);
  }

  constructor(
    private fb: FormBuilder,
    private orderService: OrderService,
    private customerService: CustomerService,
    private dealService: DealService,
    private designService: DesignService,
    private userService: UserManagementService,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.orderForm = this.fb.group({
      customerId: [''],
      dealId: [''],
      orderDate: [this.formatDate(new Date())],
      completionDate: [''],
      returnDate: [''],
      assignedToUserId: [''],
      designerUserId: [''],
      deliveryContactName: [''],
      deliveryPhone: [''],
      deliveryAddress: [''],
      deliveryWard: [''],
      deliveryDistrict: [''],
      deliveryCity: [''],
      discountPercent: [0, [Validators.min(0), Validators.max(100)]],
      taxPercent: [10, [Validators.min(0), Validators.max(100)]],
      shippingFee: [0, [Validators.min(0)]],
      paymentMethod: [''],
      styleNotes: [''],
      personNamesBySize: [''],
      giftItems: [''],
      internalNotes: [''],
      customerNotes: [''],
      productInfo: this.fb.group({
        productName: ['', Validators.required],
        material: [''],
        colorMain: [''],
        colorSub: [''],
        styleForm: [''],
        styleSpec: [''],
        unitPrice: [0, [Validators.min(0)]],
        itemDiscountPercent: [0, [Validators.min(0), Validators.max(100)]],
      }),
      items: this.fb.array([])
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEditMode = true;
      this.orderId = id;
    }

    forkJoin({
      customers: this.customerService.getCustomers({ page: 1, pageSize: 200 }),
      colors: this.designService.getAllColorFabrics(),
      materials: this.designService.getActiveShirtComponentsByType(ComponentType.Fabric),
      users: this.userService.getUsers({ page: 1, pageSize: 200, isActive: true }),
      designers: this.userService.getUsers({ page: 1, pageSize: 200, isActive: true, role: 'Designer' })
    }).subscribe({
      next: (res) => {
        this.customers = res.customers?.items || [];
        this.colorFabrics = res.colors || [];
        this.materials = res.materials || [];
        this.users = res.users?.items || [];
        this.designers = res.designers?.items || [];

        if (this.isEditMode) {
          this.loadOrder();
        } else {
          const currentUser = this.authService.getCurrentUser();
          if (currentUser) {
            this.orderForm.get('assignedToUserId')?.setValue(currentUser.id);
          }
        }
      }
    });
  }

  get items(): FormArray {
    return this.orderForm.get('items') as FormArray;
  }

  loadDealsForCustomer(customerId: string): void {
    if (!customerId) { this.filteredDeals = []; return; }
    this.dealService.getDeals({ customerId, page: 1, pageSize: 50 }).subscribe({
      next: (response) => { this.filteredDeals = response?.items || []; }
    });
  }

  getCurrentUserDisplayName(): string {
    const user = this.authService.getCurrentUser();
    return user ? `${user.firstName} ${user.lastName}` : '';
  }

  getCustomerDisplayName(customer: Customer): string {
    return customer.name + (customer.companyName ? ` (${customer.companyName})` : '');
  }

  onCustomerInput(event: Event): void {
    const inputVal = (event.target as HTMLInputElement).value;
    this.customerSearchText = inputVal;
    this.showCustomerDropdown = true;
    const q = inputVal.toLowerCase();
    this.filteredCustomers = q
      ? this.customers.filter(c =>
          c.name.toLowerCase().includes(q) ||
          (c.companyName ?? '').toLowerCase().includes(q)
        )
      : this.customers;
    this.orderForm.get('customerId')?.setValue('');
    this.orderForm.get('dealId')?.setValue('');
    this.filteredDeals = [];
  }

  onCustomerFocus(): void {
    const q = this.customerSearchText.toLowerCase();
    this.filteredCustomers = q
      ? this.customers.filter(c =>
          c.name.toLowerCase().includes(q) ||
          (c.companyName ?? '').toLowerCase().includes(q)
        )
      : this.customers;
    this.showCustomerDropdown = true;
  }

  onCustomerBlur(): void {
    setTimeout(() => { this.showCustomerDropdown = false; }, 150);
  }

  selectCustomer(customer: Customer): void {
    this.customerSearchText = this.getCustomerDisplayName(customer);
    this.orderForm.get('customerId')?.setValue(customer.id);
    this.orderForm.get('customerId')?.markAsTouched();
    this.showCustomerDropdown = false;
    this.onCustomerChange();
  }

  onCustomerChange(): void {
    const customerId = this.orderForm.get('customerId')?.value;
    this.orderForm.get('dealId')?.setValue('');
    this.loadDealsForCustomer(customerId);
    const customer = this.customers.find(c => c.id === customerId);
    if (customer) {
      this.orderForm.patchValue({
        deliveryAddress: customer.address || '',
        deliveryCity: customer.city || '',
        deliveryPhone: customer.phone || '',
        deliveryContactName: customer.name || ''
      });
    }
  }

  loadOrder(): void {
    if (!this.orderId) return;
    this.isLoading = true;
    this.orderService.getOrder(this.orderId).subscribe({
      next: (order) => {
        this.orderForm.patchValue({
          customerId: order.customerId,
          dealId: order.dealId || '',
          orderDate: this.formatDate(new Date(order.orderDate)),
          completionDate: order.completionDate ? this.formatDate(new Date(order.completionDate)) : '',
          returnDate: order.returnDate ? this.formatDate(new Date(order.returnDate)) : '',
          assignedToUserId: order.assignedToUserId || '',
          designerUserId: order.designerUserId || '',
          deliveryAddress: order.deliveryAddress || '',
          deliveryCity: order.deliveryCity || '',
          deliveryDistrict: order.deliveryDistrict || '',
          deliveryWard: order.deliveryWard || '',
          deliveryPhone: order.deliveryPhone || '',
          deliveryContactName: order.deliveryContactName || '',
          discountPercent: order.discountPercent || 0,
          taxPercent: order.taxPercent || 10,
          shippingFee: order.shippingFee || 0,
          paymentMethod: order.paymentMethod || '',
          styleNotes: order.styleNotes || '',
          personNamesBySize: order.personNamesBySize || '',
          giftItems: order.giftItems || '',
          internalNotes: order.internalNotes || '',
          customerNotes: order.customerNotes || ''
        });

        // Customer display name
        const selectedCustomer = this.customers.find(c => c.id === order.customerId);
        if (selectedCustomer) {
          this.customerSearchText = this.getCustomerDisplayName(selectedCustomer);
        } else if (order.customerName) {
          this.customerSearchText = order.customerName;
        }
        this.loadDealsForCustomer(order.customerId);

        // Populate productInfo from first item
        const firstItem = order.items[0];
        if (firstItem) {
          // Parse styleNotes back to productInfo fields
          const styleMap: Record<string, string> = {};
          (order.styleNotes || '').split('|').forEach(p => {
            const i = p.indexOf(':');
            if (i > -1) styleMap[p.slice(0, i).trim().toLowerCase()] = p.slice(i + 1).trim();
          });

          this.orderForm.get('productInfo')?.patchValue({
            productName: firstItem.productName || '',
            material: styleMap['chất liệu'] || firstItem.material || '',
            colorMain: styleMap['màu chính'] || firstItem.color || '',
            colorSub: styleMap['màu phối'] || '',
            styleForm: styleMap['form'] || '',
            styleSpec: styleMap['quy cách'] || '',
            unitPrice: firstItem.unitPrice || 0,
            itemDiscountPercent: firstItem.discountPercent || 0,
          });
        }

        // Populate sizeQty from items
        this.sizeQty = {};
        order.items.forEach(item => {
          if (item.size) {
            const sz = item.size.trim().toUpperCase();
            this.sizeQty[sz] = (this.sizeQty[sz] || 0) + item.quantity;
          }
        });

        this.items.clear();
        this.isLoading = false;
      },
      error: () => { this.errorMessage = 'Không thể tải thông tin đơn hàng'; this.isLoading = false; }
    });
  }

  createItemFormGroup(): FormGroup {
    return this.fb.group({
      productName: ['', [Validators.required]],
      productCode: [''],
      description: [''],
      size: [''],
      color: [''],
      material: [''],
      quantity: [1, [Validators.required, Validators.min(1)]],
      unit: ['cái'],
      unitPrice: [0, [Validators.required, Validators.min(0)]],
      discountPercent: [0, [Validators.min(0), Validators.max(100)]],
      notes: ['']
    });
  }

  addItem(): void { this.items.push(this.createItemFormGroup()); }
  removeItem(index: number): void { if (this.items.length > 1) this.items.removeAt(index); }

  calculateItemTotal(index: number): number {
    const item = this.items.at(index);
    const qty = item.get('quantity')?.value || 0;
    const price = item.get('unitPrice')?.value || 0;
    const disc = item.get('discountPercent')?.value || 0;
    return qty * price * (1 - disc / 100);
  }

  calculateSubTotal(): number {
    let total = 0;
    for (let i = 0; i < this.items.length; i++) total += this.calculateItemTotal(i);
    return total;
  }

  calculateDiscountAmount(): number {
    return this.calculateSubTotal() * ((this.orderForm.get('discountPercent')?.value || 0) / 100);
  }

  calculateTaxAmount(): number {
    return (this.calculateSubTotal() - this.calculateDiscountAmount()) * ((this.orderForm.get('taxPercent')?.value || 0) / 100);
  }

  calculateTotal(): number {
    return this.calculateSubTotal() - this.calculateDiscountAmount() + this.calculateTaxAmount() + (this.orderForm.get('shippingFee')?.value || 0);
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }

  formatDate(date: Date): string { return date.toISOString().split('T')[0]; }

  onSubmit(): void {
    if (this.orderForm.invalid) { this.orderForm.markAllAsTouched(); return; }
    this.isLoading = true;
    this.errorMessage = '';

    const f = this.orderForm.value;
    const orderData = {
      customerId: f.customerId || undefined,
      customerName: !f.customerId ? this.customerSearchText || undefined : undefined,
      dealId: f.dealId || null,
      orderDate: f.orderDate ? new Date(f.orderDate) : undefined,
      completionDate: f.completionDate ? new Date(f.completionDate) : undefined,
      returnDate: f.returnDate ? new Date(f.returnDate) : undefined,
      assignedToUserId: f.assignedToUserId || undefined,
      designerUserId: f.designerUserId || undefined,
      deliveryAddress: f.deliveryAddress,
      deliveryCity: f.deliveryCity,
      deliveryDistrict: f.deliveryDistrict,
      deliveryWard: f.deliveryWard,
      deliveryPhone: f.deliveryPhone,
      deliveryContactName: f.deliveryContactName,
      discountPercent: f.discountPercent,
      taxPercent: f.taxPercent,
      shippingFee: f.shippingFee,
      paymentMethod: f.paymentMethod,
      styleNotes: this.buildStyleNotes(f),
      personNamesBySize: f.personNamesBySize || undefined,
      giftItems: f.giftItems || undefined,
      internalNotes: f.internalNotes,
      customerNotes: f.customerNotes,
      items: this.SIZE_LIST
        .filter(s => (this.sizeQty[s] || 0) > 0)
        .map((s, index) => ({
          productName: f.productInfo?.productName || 'Sản phẩm',
          size: s,
          quantity: this.sizeQty[s],
          unit: 'cái',
          unitPrice: f.productInfo?.unitPrice || 0,
          discountPercent: f.productInfo?.itemDiscountPercent || 0,
          sortOrder: index
        } as CreateOrderItemRequest))
    };

    const obs$ = this.isEditMode && this.orderId
      ? this.orderService.updateOrder(this.orderId, { ...orderData, id: this.orderId })
      : this.orderService.createOrder(orderData);

    obs$.subscribe({
      next: (order) => this.router.navigate(['/orders', order.id]),
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.message || 'Lưu thất bại. Vui lòng thử lại.';
      }
    });
  }

  cancel(): void { this.router.navigate(['/orders']); }

  private buildStyleNotes(f: any): string {
    const pi = f.productInfo || {};
    const parts: string[] = [];
    if (pi.material)   parts.push(`Chất liệu: ${pi.material}`);
    if (pi.colorMain)  parts.push(`Màu chính: ${pi.colorMain}`);
    if (pi.colorSub)   parts.push(`Màu phối: ${pi.colorSub}`);
    if (pi.styleForm)  parts.push(`Form: ${pi.styleForm}`);
    if (pi.styleSpec)  parts.push(`Quy cách: ${pi.styleSpec}`);
    if (f.styleNotes)  parts.push(f.styleNotes);
    return parts.join(' | ') || '';
  }
  get f() { return this.orderForm.controls; }
}
