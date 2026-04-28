export interface Province {
  code: string;
  name: string;
  fullName: string;
  type: string;
}

export interface Ward {
  code: string;
  name: string;
  fullName: string;
  type: string;
  provinceCode: string;
}
