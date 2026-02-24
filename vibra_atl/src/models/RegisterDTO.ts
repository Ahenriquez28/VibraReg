export interface RegisterDTO {
    fullName: string;
    email: string;
    school: string;
    gpa: string;
    hasGroup: boolean;
    groupName?: string;  // NEW
    resume?: File;
}