// Production deployment cho domain crmsolution.net.
// Build: ng build --configuration crmsolution
export const environment = {
  production: true,
  apiUrl: '/api',
  appName: 'CRM - Quản lý Khách hàng',
  defaultLanguage: 'vi',
  branding: {
    companyName: 'TEN_CONG_TY',
    headerLine1: 'TEN_DONG_1',
    headerLine2: 'TEN_DONG_2',
    logoPath: '/assets/logo-crmsolution.png',
    logoAlt: 'CRM',
    canvasCompanyName: 'TEN_CONG_TY_ASCII',
    address: 'DIA_CHI',
    hotline: 'HOTLINE',
    website: 'crmsolution.net'
  }
};
