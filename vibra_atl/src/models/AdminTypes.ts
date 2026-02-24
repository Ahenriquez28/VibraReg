// types/admin.types.ts
export interface Student {
  id: number;
  fullName: string;
  email: string;
  school: string;
  gpa: string;
  hasGroup: boolean;
  resumePath?: string;
  isPresent: boolean;
  status: string;
}

export interface Team {
  teamId: number;
  groupName: string;
  teamFull: boolean;
  students: Student[];
}

export interface TeamChange {
  studentId: number;
  teamId: number | null;
}

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  teams?: T;
}